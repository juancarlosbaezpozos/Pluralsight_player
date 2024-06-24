using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain;

public class VersionMigrator
{
    private readonly ICourseRepository courseRepository;

    private readonly CourseNameToIds courseNameToids;

    private readonly IDownloadFileLocator fileLocator;

    private readonly ISettingsRepository settingsRepository;

    private readonly IClipViewRepository clipViewRepository;

    private readonly ICourseAccessRepository courseAccessRepository;

    private readonly ICourseProgressRepository courseProgressRepository;

    public VersionMigrator(ICourseRepository courseRepository, CourseNameToIds courseNameToids, IDownloadFileLocator fileLocator, ISettingsRepository settingsRepository, IClipViewRepository clipViewRepository, ICourseAccessRepository courseAccessRepository, ICourseProgressRepository courseProgressRepository)
    {
        this.courseRepository = courseRepository;
        this.courseNameToids = courseNameToids;
        this.fileLocator = fileLocator;
        this.settingsRepository = settingsRepository;
        this.clipViewRepository = clipViewRepository;
        this.courseAccessRepository = courseAccessRepository;
        this.courseProgressRepository = courseProgressRepository;
    }

    public async Task PerformMigration()
    {
        List<CourseDetail> allCoursesV2 = courseRepository.LoadAllDownloaded();
        List<CourseDetail> allCoursesV1 = courseRepository.LoadAllDownloaded();
        List<string> courseNames = allCoursesV2.Select((CourseDetail c) => c.Name).ToList();
        NameToIdsResult nameToIdsResult = await courseNameToids.Execute(courseNames);
        if (nameToIdsResult != null)
        {
            courseRepository.MigrationStatus = V2MigrationStatus.InProgress;
            List<ClipView> clipViewsV = clipViewRepository.LoadAll();
            for (int i = 0; i < allCoursesV2.Count; i++)
            {
                CourseDetail courseDetail = allCoursesV2[i];
                CourseDetail courseDetail2 = allCoursesV1[i];
                CourseIdsDao courseIdsDao = nameToIdsResult.Collection[i];
                if (courseIdsDao != null && courseIdsDao.Modules.Count((ModuleIdsDao m) => m == null) <= 0 && courseIdsDao.Modules.Count((ModuleIdsDao m) => m != null && m.Clips.Count((string c) => c == null) > 0) <= 0)
                {
                    courseDetail.Name = courseIdsDao.Id;
                    courseDetail.UrlSlug = courseDetail2.Name;
                    MigrateCourseModules(courseDetail, courseIdsDao, clipViewsV);
                    courseAccessRepository.ClearAll();
                    courseProgressRepository.ClearAll();
                    RenameCourseVideosOnDisk(courseDetail2, courseDetail);
                    MoveCourseImage(courseDetail2, courseDetail);
                    CleanupCourseV1Folder(courseDetail2);
                }
            }
            settingsRepository.UpdateApiVersion("v2");
        }
        courseRepository.MigrationStatus = V2MigrationStatus.Complete;
    }

    private void CleanupCourseV1Folder(CourseDetail courseV1)
    {
        try
        {
            string folderForCourseDownloads = fileLocator.GetFolderForCourseDownloads(courseV1);
            if (Directory.Exists(folderForCourseDownloads))
            {
                Directory.Delete(folderForCourseDownloads, recursive: true);
            }
        }
        catch (Exception)
        {
        }
    }

    private void MoveCourseImage(CourseDetail courseV1, CourseDetail courseV2)
    {
        try
        {
            string filenameForCourseImage = fileLocator.GetFilenameForCourseImage(courseV1);
            string filenameForCourseImage2 = fileLocator.GetFilenameForCourseImage(courseV2);
            if (!File.Exists(filenameForCourseImage2) && File.Exists(filenameForCourseImage))
            {
                File.Move(filenameForCourseImage, filenameForCourseImage2);
            }
        }
        catch (Exception)
        {
        }
    }

    private void RenameCourseVideosOnDisk(CourseDetail courseV1, CourseDetail courseV2)
    {
        for (int i = 0; i < courseV1.Modules.Count; i++)
        {
            Module module = courseV1.Modules[i];
            Module module2 = courseV2.Modules[i];
            for (int j = 0; j < module2.Clips.Count; j++)
            {
                try
                {
                    Clip clip = module.Clips[j];
                    FileInfo clipFileInfo = fileLocator.GetClipFileInfo(courseV1, module, clip);
                    Clip clip2 = module2.Clips[j];
                    FileInfo clipFileInfo2 = fileLocator.GetClipFileInfo(courseV2, module2, clip2);
                    clipFileInfo2.Directory.Create();
                    if (!clipFileInfo2.Exists && clipFileInfo.Exists)
                    {
                        clipFileInfo.MoveTo(clipFileInfo2.FullName);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }

    private void MigrateCourseModules(CourseDetail courseV2, CourseIdsDao courseIds, IList<ClipView> clipViewsV1)
    {
        for (int i = 0; i < courseV2.Modules.Count; i++)
        {
            Module moduleV2 = courseV2.Modules[i];
            ModuleIdsDao moduleIdsDao = courseIds.Modules[i];
            foreach (ClipView item in clipViewsV1.Where((ClipView c) => c.CourseName == courseV2.UrlSlug && c.ModuleName == moduleV2.Name && c.AuthorHandle == moduleV2.AuthorHandle))
            {
                clipViewRepository.Migrate(item, courseV2.Name, moduleIdsDao.Id);
            }
            moduleV2.Name = moduleIdsDao.Id;
            for (int j = 0; j < moduleV2.Clips.Count; j++)
            {
                Clip clip = moduleV2.Clips[j];
                string name = moduleIdsDao.Clips[j];
                clip.Name = name;
            }
        }
        courseRepository.SaveForDownload(courseV2);
        courseRepository.Delete(courseV2.UrlSlug);
    }
}
