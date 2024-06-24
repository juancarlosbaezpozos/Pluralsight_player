using System.Collections.Generic;

namespace Pluralsight.Domain;

public class CourseProgressDao
{
    public string CourseId { get; set; }

    public List<ModuleProgressDao> ViewedModules { get; set; }

    public LastViewedClipInfoDao LastViewed { get; set; }
}
