using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain;

public class ClipDownloader
{
    private class DownloadForOfflineRequest
    {
        public string CourseId { get; set; }

        public string ClipId { get; set; }

        public string AspectRatio { get; set; }
    }

    public readonly ClipDownloadInfo ClipDownloadInfo;

    private readonly IDownloadFileLocator fileLocator;

    private readonly IRestHelper pluralsightHelper;

    private bool currentDownloadCancelled;

    public DownloadStatus DownloadStatus;

    private const int DownloadBufferByteSize = 4096;

    public event Action<ClipDownloader, DownloadStatus> DownloadCompleted;

    public ClipDownloader(IDownloadFileLocator fileLocator, IRestHelper pluralsightHelper, ClipDownloadInfo clipDownloadInfo)
    {
        this.fileLocator = fileLocator;
        this.pluralsightHelper = pluralsightHelper;
        ClipDownloadInfo = clipDownloadInfo;
    }

    public async Task<bool> StartDownload()
    {
        if (currentDownloadCancelled)
        {
            return true;
        }
        DownloadForOfflineRequest request = new()
        {
            CourseId = ClipDownloadInfo.Course.Name,
            ClipId = ClipDownloadInfo.Clip.Name,
            AspectRatio = GetFormatString(ClipDownloadInfo.Clip)
        };
        RestResponse<DownloadUrlResponse> restResponse = await pluralsightHelper.AuthenticatedPost<DownloadUrlResponse, DownloadForOfflineRequest>("/library/videos/offline", request);
        if (restResponse.StatusCode != HttpStatusCode.OK || restResponse.Data == null)
        {
            return false;
        }
        await DownloadClipTryEachOption(restResponse.Data, ClipDownloadInfo.Course, ClipDownloadInfo.Module, ClipDownloadInfo.Clip);
        return true;
    }

    private async Task DownloadClipTryEachOption(DownloadUrlResponse options, CourseDetail course, Module module, Clip clip)
    {
        DownloadStatus downloadStatus = DownloadStatus.Unknown;
        foreach (UrlOptionResponse rankedOption in options.RankedOptions)
        {
            downloadStatus = await TryToDownloadClip(rankedOption.Url, course, module, clip);
            if (downloadStatus == DownloadStatus.Success)
            {
                break;
            }
        }
        OnDownloadCompleted(downloadStatus);
    }

    private async Task<DownloadStatus> TryToDownloadClip(string url, CourseDetail course, Module module, Clip clip)
    {
        HttpClient client = HttpClientFactory.GetClient();
        try
        {
            HttpResponseMessage httpResponseMessage = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                return DownloadStatus.NetworkError;
            }
            return await ProcessDownload(httpResponseMessage, course, module, clip);
        }
        catch (HttpRequestException)
        {
            DownloadStatus = DownloadStatus.NetworkError;
            return DownloadStatus;
        }
        catch (IOException ex2) when (ex2.InnerException is Win32Exception)
        {
            DownloadStatus = DownloadStatus.NetworkError;
            return DownloadStatus;
        }
        catch (IOException)
        {
            DownloadStatus = DownloadStatus.DiskError;
            return DownloadStatus;
        }
        catch (Exception)
        {
            DownloadStatus = DownloadStatus.UnknownError;
            return DownloadStatus;
        }
    }

    private async Task<DownloadStatus> ProcessDownload(HttpResponseMessage response, CourseDetail course, Module module, Clip clip)
    {
        byte[] buffer = new byte[4096];
        FileInfo fileInfo = fileLocator.GetClipFileInfo(course, module, clip);
        fileInfo.Directory?.Create();
        string tempFileName = fileInfo.FullName + ".tmp";
        using (Stream stream = await response.Content.ReadAsStreamAsync())
        {
            using FileStream fileStream = File.Create(tempFileName);
            long position = 0L;
            int num = await stream.ReadAsync(buffer, 0, buffer.Length);
            while (num > 0 && !currentDownloadCancelled)
            {
                position += num;
                fileStream.Write(buffer, 0, num);
                num = await stream.ReadAsync(buffer, 0, buffer.Length);
            }
        }
        if (fileInfo.Exists)
        {
            File.Delete(fileInfo.FullName);
        }
        if (!currentDownloadCancelled)
        {
            File.Move(tempFileName, fileInfo.FullName);
        }
        currentDownloadCancelled = false;
        return DownloadStatus.Success;
    }

    private void OnDownloadCompleted(DownloadStatus success)
    {
        this.DownloadCompleted?.Invoke(this, success);
    }

    private string GetFormatString(Clip clip)
    {
        if (clip.SupportsWidescreen)
        {
            return "widescreen";
        }
        return "standard";
    }

    public void CancelCurrentDownload()
    {
        currentDownloadCancelled = true;
    }
}
