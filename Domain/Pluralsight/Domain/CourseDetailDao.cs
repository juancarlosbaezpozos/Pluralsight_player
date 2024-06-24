using System.Collections.Generic;

namespace Pluralsight.Domain;

public class CourseDetailDao
{
    public CourseHeaderDao Header { get; set; }

    public string Title { get; set; }

    public List<ModuleDao> Modules { get; set; }

    public List<Tag> AudienceTags { get; set; }

    public string Description { get; set; }

    public RetiredCourse RetiredCourseInfo { get; set; }

    public string Level { get; set; }

    public string Color { get; set; }

    public List<SkillPathHeader> SkillPaths { get; set; }
}
