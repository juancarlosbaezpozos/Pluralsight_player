using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain;

public class CourseProgressFetcher
{
    private readonly IRestHelper restHelper;

    private readonly ICourseProgressRepository courseProgressRepository;

    private readonly ICourseRepository courseRepository;

    private static readonly MemoryCache LoadedCoursesCache = new MemoryCache(new MemoryCacheOptions
    {
        ExpirationScanFrequency = TimeSpan.FromSeconds(15.0)
    });

    public CourseProgressFetcher(IRestHelper restHelper, ICourseProgressRepository courseProgressRepository, ICourseRepository courseRepository)
    {
        this.restHelper = restHelper;
        this.courseProgressRepository = courseProgressRepository;
        this.courseRepository = courseRepository;
    }

    public static void ClearCache()
    {
        LoadedCoursesCache.Compact(1.0);
    }

    public async Task<CourseProgress> GetProgressForCourse(string courseName)
    {
        if (LoadedCoursesCache.TryGetValue(courseName, out var result))
        {
            return result as CourseProgress;
        }
        CourseProgress localCourseProgress = courseProgressRepository.Load(courseName);
        CourseProgress courseProgress = MergeProgress(localCourseProgress, await ProgressForCourse(courseName));
        if (courseProgress?.LastViewed != null)
        {
            courseProgressRepository.Save(courseProgress);
        }
        return LoadedCoursesCache.Set(courseName, courseProgress, TimeSpan.FromSeconds(60.0));
    }

    private CourseProgress MergeProgress(CourseProgress localCourseProgress, CourseProgress remoteProgress)
    {
        if (localCourseProgress == null)
        {
            return remoteProgress;
        }
        CourseProgress courseProgress = new CourseProgress
        {
            CourseName = localCourseProgress.CourseName,
            ViewedModules = new List<ModuleProgress>()
        };
        foreach (ModuleProgress viewedModule in remoteProgress.ViewedModules)
        {
            courseProgress.ViewedModules.Add(viewedModule);
        }
        foreach (ModuleProgress module in localCourseProgress.ViewedModules)
        {
            ModuleProgress moduleProgress = courseProgress.ViewedModules.FirstOrDefault((ModuleProgress x) => x.ModuleName == module.ModuleName);
            if (moduleProgress == null)
            {
                courseProgress.ViewedModules.Add(module);
            }
            else
            {
                moduleProgress.ViewedClipIndexes = moduleProgress.ViewedClipIndexes.Union(module.ViewedClipIndexes).ToList();
            }
        }
        if (remoteProgress.LastViewed == null || (localCourseProgress.LastViewed != null && localCourseProgress.LastViewed.ViewTime > remoteProgress.LastViewed.ViewTime))
        {
            courseProgress.LastViewed = localCourseProgress.LastViewed;
        }
        else
        {
            courseProgress.LastViewed = remoteProgress.LastViewed;
        }
        return courseProgress;
    }

    private async Task<CourseProgress> ProgressForCourse(string courseName)
    {
        RestResponse<CourseProgressDao> result = await restHelper.AuthenticatedGet<CourseProgressDao>("/user/courses/" + courseName + "/progress");
        if (result.StatusCode != HttpStatusCode.OK)
        {
            return new CourseProgress
            {
                CourseName = courseName,
                ViewedModules = new List<ModuleProgress>()
            };
        }
        CourseDetail courseDetail = courseRepository.Load(courseName);
        if (courseDetail == null)
        {
            courseDetail = (await ObjectFactory.Get<CourseDownloadCommand>().GetCourseDetail(courseName)).Course;
        }
        return CourseProgressMapper.ToCourseProgress(result.Data, courseDetail);
    }
}
