using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain;

public class DownloadManager : IDownloadQueue
{
    private const int MaxDownloadRetry = 5;

    private readonly int maxConcurrentDownloads;

    private readonly IDownloadFileLocator fileLocator;

    private readonly IRestHelper restHelper;

    private readonly ConcurrentDictionary<CourseDetail, int> courseDownloadProgress;

    private readonly CourseImageDownloader imageDownloader;

    private readonly TranscriptSrtDownloader srtDownloader;

    private readonly TranscriptDownloader transcriptDownloader;

    private readonly IProgress<CourseProgressUpdate> courseProgressUpdated;

    private readonly IProgress<QueueProgressUpdate> queueProgressUpdated;

    private readonly ConcurrentQueue<ClipDownloader> clipsDownloadQueue;

    private readonly ConcurrentBag<Task<bool>> currentClipDownloadTasks;

    public static bool CanShowFirstClipDownloadFailedWindow;

    public Progress<CourseProgressUpdate> CourseProgressUpdated => (Progress<CourseProgressUpdate>)courseProgressUpdated;

    public Progress<QueueProgressUpdate> QueueProgressUpdated => (Progress<QueueProgressUpdate>)queueProgressUpdated;

    public bool MaxClipsDownloading => currentClipDownloadTasks.Count((Task<bool> t) => !t.IsCompleted) >= maxConcurrentDownloads;

    public bool ClipsDownloading => currentClipDownloadTasks.Any((Task<bool> t) => !t.IsCompleted);

    public event Action FirstClipDownloadFailed;

    public DownloadManager(IDownloadFileLocator fileLocator, IRestHelper restHelper, ITranscriptRepository transcriptRepository, ISettingsRepository settingsRepository, ICourseRepository courseRepository)
    {
        this.fileLocator = fileLocator;
        this.restHelper = restHelper;
        courseDownloadProgress = new ConcurrentDictionary<CourseDetail, int>();
        imageDownloader = new CourseImageDownloader(fileLocator);
        transcriptDownloader = new TranscriptDownloader(restHelper, transcriptRepository, settingsRepository, courseRepository);
        srtDownloader = new TranscriptSrtDownloader(fileLocator, transcriptRepository);
        courseProgressUpdated = new Progress<CourseProgressUpdate>();
        queueProgressUpdated = new Progress<QueueProgressUpdate>();
        clipsDownloadQueue = new ConcurrentQueue<ClipDownloader>();
        currentClipDownloadTasks = new ConcurrentBag<Task<bool>>();
        maxConcurrentDownloads = Math.Max(5, Environment.ProcessorCount * 2);
    }

    private void ClipDownloader_DownloadCompleted(ClipDownloader clipDownloader, DownloadStatus downloadSuccess)
    {
        if (downloadSuccess != DownloadStatus.Success)
        {
            if (clipDownloader.ClipDownloadInfo.IsFirstClipInCourse)
            {
                this.FirstClipDownloadFailed?.Invoke();
            }
            if (++clipDownloader.ClipDownloadInfo.RetryCount < 5)
            {
                clipsDownloadQueue.Enqueue(clipDownloader);
            }
            else
            {
                IncrementAndUpdateProgress(clipDownloader.ClipDownloadInfo);
            }
        }
        else
        {
            IncrementAndUpdateProgress(clipDownloader.ClipDownloadInfo);
        }
    }

    private void IncrementAndUpdateProgress(ClipDownloadInfo clipDownloadInfo)
    {
        if (courseDownloadProgress.ContainsKey(clipDownloadInfo.Course))
        {
            courseDownloadProgress[clipDownloadInfo.Course]++;
            BroadcastProgressForCourse(clipDownloadInfo.Course);
        }
    }

    public async Task QueueCourseForDownload(CourseDetail course)
    {
        if (courseDownloadProgress.Keys.Contains(course))
        {
            return;
        }
        courseDownloadProgress.TryAdd(course, 0);
        foreach (Module module in course.Modules)
        {
            foreach (Clip clip in module.Clips)
            {
                if (fileLocator.GetClipFileInfo(course, module, clip).Exists)
                {
                    courseDownloadProgress[course]++;
                    continue;
                }
                ClipDownloadInfo clipDownloadInfo = new ClipDownloadInfo
                {
                    Course = course,
                    Module = module,
                    Clip = clip
                };
                ClipDownloader clipDownloader = new ClipDownloader(fileLocator, restHelper, clipDownloadInfo);
                clipDownloader.DownloadCompleted += ClipDownloader_DownloadCompleted;
                clipsDownloadQueue.Enqueue(clipDownloader);
            }
        }
        BroadcastProgressForCourse(course);
        await Task.WhenAll(new List<Task>
        {
            imageDownloader.DownloadCourseImage(course),
            transcriptDownloader.DownloadCourseTranscripts(new List<CourseDetail> { course }),
            StartOrContinueDownloading()
        });
        await Task.WhenAll(srtDownloader.Generate(course));
    }

    private async Task StartOrContinueDownloading()
    {
        if (MaxClipsDownloading)
        {
            return;
        }
        ClipDownloader result;
        while (clipsDownloadQueue.TryDequeue(out result))
        {
            if (result != null)
            {
                Task<bool> item = result.StartDownload();
                currentClipDownloadTasks.Add(item);
                List<Task<bool>> list = currentClipDownloadTasks.Where((Task<bool> t) => !t.IsCompleted).ToList();
                if (list.Count >= maxConcurrentDownloads)
                {
                    await Task.WhenAny(list);
                }
            }
        }
        await Task.WhenAll(currentClipDownloadTasks);
    }

    public bool IsCourseQueuedForDownload(CourseDetail course)
    {
        if (!courseDownloadProgress.ContainsKey(course))
        {
            return false;
        }
        return courseDownloadProgress[course] == 0;
    }

    public bool IsCourseCurrentlyDownloading(CourseDetail course)
    {
        if (!courseDownloadProgress.ContainsKey(course))
        {
            return false;
        }
        int num = courseDownloadProgress[course];
        int num2 = course.Modules.Sum((Module x) => x.Clips.Count);
        if (num > 0)
        {
            return num < num2;
        }
        return false;
    }

    public void RemoveDownloadedCoursesNotInLocalDatabase(List<CourseDetail> courses)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(fileLocator.GetFolderForCoursesDownloads());
        List<string> list = courses.Select((CourseDetail x) => x.Name).ToList();
        DirectoryInfo[] directories = directoryInfo.GetDirectories();
        foreach (DirectoryInfo directoryInfo2 in directories)
        {
            if (!list.Contains(directoryInfo2.Name))
            {
                try
                {
                    directoryInfo2.Delete(recursive: true);
                }
                catch
                {
                }
            }
        }
    }

    public void DeleteCourse(CourseDetail course)
    {
        foreach (ClipDownloader item in clipsDownloadQueue.Where((ClipDownloader c) => c.ClipDownloadInfo.Course.Equals(course)))
        {
            item.CancelCurrentDownload();
        }
        if (courseDownloadProgress.ContainsKey(course))
        {
            courseDownloadProgress.TryRemove(course, out var _);
        }
        BroadcastProgressForCourse(course);
        try
        {
            string folderForCourseDownloads = fileLocator.GetFolderForCourseDownloads(course);
            if (Directory.Exists(folderForCourseDownloads))
            {
                Directory.Delete(folderForCourseDownloads, recursive: true);
            }
        }
        catch (IOException)
        {
        }
    }

    public void ClearQueue()
    {
        ClipDownloader result;
        while (clipsDownloadQueue.TryDequeue(out result))
        {
            result?.CancelCurrentDownload();
        }
    }

    private void BroadcastProgressForCourse(CourseDetail course)
    {
        if (course != null && courseDownloadProgress.ContainsKey(course))
        {
            double percent = (double)courseDownloadProgress[course] / (double)course.Modules.Sum((Module x) => x.Clips.Count);
            courseProgressUpdated?.Report(new CourseProgressUpdate
            {
                Course = course,
                Percent = percent
            });
        }
        else
        {
            courseProgressUpdated?.Report(new CourseProgressUpdate
            {
                Course = course,
                Percent = 0.0
            });
        }
        double num = (double)courseDownloadProgress.Values.Sum() / (double)courseDownloadProgress.Keys.Sum((CourseDetail x) => x.Modules.Sum((Module y) => y.Clips.Count));
        int num2 = courseDownloadProgress.Count((KeyValuePair<CourseDetail, int> x) => x.Value == x.Key.Modules.Sum((Module y) => y.Clips.Count));
        if (double.IsNaN(num))
        {
            num = 1.0;
        }
        queueProgressUpdated?.Report(new QueueProgressUpdate
        {
            Percent = num,
            Index = num2 + 1,
            Count = courseDownloadProgress.Count,
            Course = course
        });
    }
}
