using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain.WPF.Persistance;

public class DownloadFileLocator : IDownloadFileLocator
{
    public FileInfo GetClipFileInfo(CourseDetail course, Module module, Clip clip)
    {
        var clipName = FixName($"{clip.Index} - {clip.Title}");
        return new FileInfo(string.Concat(GetFolderForCourseDownloads(course) + "\\" + GetModuleHash(module), "\\", clipName, ".mp4"));
    }

    public string GetFilenameForCourseImage(CourseDetail course)
    {
        return GetFolderForCourseDownloads(course) + "\\image.jpg";
    }

    public string GetFolderForCoursesDownloads()
    {
        string text = GetSetting("DownloadLocationRoot");
        if (string.IsNullOrWhiteSpace(text) || !Path.IsPathRooted(text))
        {
            text = DiskLocations.DefaultDownloadLocationRoot();
        }
        string folder;
        try
        {
            folder = Path.GetFullPath(text);
        }
        catch (Exception)
        {
            folder = DiskLocations.DefaultDownloadLocationRoot();
        }
        return SettingsNames.GetCoursesDownloadLocation(folder);
    }

    public List<string> GetCoursesInDownloadsFolder()
    {
        List<string> list = new List<string>();
        foreach (string item in Directory.EnumerateDirectories(GetFolderForCoursesDownloads()))
        {
            list.Add(new DirectoryInfo(item).Name);
        }
        return list;
    }

    private string GetSetting(string settingName)
    {
        return ObjectFactory.Get<ISettingsRepository>().Load(settingName);
    }

    public string GetFolderForCourseDownloads(CourseDetail course)
    {
        var name = FixName(course.Title);
        return Path.Combine(GetFolderForCoursesDownloads(), name);
    }

    private string GetModuleHash(Module module)
    {
        var name = FixName(module.Title);
        return name.Trim();
    }

    public bool IsCourseDownloadComplete(CourseDetail course)
    {
        return course.Modules.SelectMany((Module module) => module.Clips.Select((Clip clip) => GetClipFileInfo(course, module, clip))).All((FileInfo x) => x.Exists);
    }

    public long CourseSizeOnDisk(CourseDetail course)
    {
        return course.Modules.Sum((Module module) => module.Clips.Sum(delegate (Clip clip)
        {
            FileInfo clipFileInfo = GetClipFileInfo(course, module, clip);
            return (!clipFileInfo.Exists) ? 0 : clipFileInfo.Length;
        }));
    }

    public long AvailableFreeSpaceOnDisk()
    {
        try
        {
            return new DriveInfo(new DirectoryInfo(GetFolderForCoursesDownloads()).Root.Name).AvailableFreeSpace;
        }
        catch (Exception)
        {
            return 0L;
        }
    }

    public bool DownloadLocationExists()
    {
        try
        {
            Directory.CreateDirectory(GetFolderForCoursesDownloads());
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    string FixName(string name)
    {
        var illegal = name;
        var invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

        foreach (char c in invalid)
        {
            illegal = illegal.Replace(c.ToString(), "");
        }

        return illegal;
    }
}
