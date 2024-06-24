using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain;

public class CourseImageDownloader
{
    private readonly IDownloadFileLocator downloadLocator;

    public CourseImageDownloader(IDownloadFileLocator downloadLocator)
    {
        this.downloadLocator = downloadLocator;
    }

    public async Task<bool> DownloadCourseImage(CourseDetail course)
    {
        FileInfo file = new FileInfo(downloadLocator.GetFilenameForCourseImage(course));
        file.Directory?.Create();
        try
        {
            if (!file.Exists)
            {
                HttpClient client = HttpClientFactory.GetClient();
                string requestUri = course.ImageUrl ?? course.DefaultImageUrl;
                Stream stream = await client.GetStreamAsync(requestUri);
                using FileStream outStream = file.OpenWrite();
                await stream.CopyToAsync(outStream);
            }
        }
        catch
        {
        }
        return file.Exists;
    }
}
