using System.Collections.Generic;

namespace Pluralsight.Domain;

public class CourseProgress
{
    public string CourseName { get; set; }

    public List<ModuleProgress> ViewedModules { get; set; }

    public LastViewedClipInfo LastViewed { get; set; }
}
