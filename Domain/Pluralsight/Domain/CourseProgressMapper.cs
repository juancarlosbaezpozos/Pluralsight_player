using System.Linq;

namespace Pluralsight.Domain;

public class CourseProgressMapper
{
    public static CourseProgress ToCourseProgress(CourseProgressDao dao, CourseDetail courseDetail)
    {
        Module module = courseDetail.Modules.FirstOrDefault((Module m) => m.Name == dao.LastViewed?.ModuleId);
        LastViewedClipInfo lastViewed = null;
        if (module != null && dao.LastViewed != null)
        {
            lastViewed = new LastViewedClipInfo
            {
                ModuleName = dao.LastViewed.ModuleId,
                ClipModuleIndex = module.Clips.First((Clip c) => c.Name == dao.LastViewed.ClipId).Index,
                ViewTime = dao.LastViewed.ViewTime
            };
        }
        return new CourseProgress
        {
            CourseName = dao.CourseId,
            ViewedModules = (from x in dao.ViewedModules?.Select((ModuleProgressDao m) => ToModuleProgress(m, courseDetail))
                             where x != null
                             select x).ToList(),
            LastViewed = lastViewed
        };
    }

    private static ModuleProgress ToModuleProgress(ModuleProgressDao dao, CourseDetail courseDetail)
    {
        Module module = courseDetail.Modules.FirstOrDefault((Module m) => m.Name == dao.ModuleId);
        if (module == null)
        {
            return null;
        }
        return new ModuleProgress
        {
            ModuleName = dao.ModuleId,
            ViewedClipIndexes = dao.ViewedClipIds.Select((string cid) => module.Clips.First((Clip c) => c.Name == cid).Index).ToList()
        };
    }
}
