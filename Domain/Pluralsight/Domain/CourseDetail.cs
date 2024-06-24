using System;
using System.Collections.Generic;
using System.Linq;

namespace Pluralsight.Domain;

public class CourseDetail : CourseHeader
{
    public List<Author> Authors { get; set; }

    public List<Module> Modules { get; set; }

    public List<Tag> AudienceTags { get; set; }

    public string ShortDescription { get; set; }

    public string Description { get; set; }

    public TimeSpan DurationInMilliseconds { get; set; }

    public bool HasTranscript { get; set; }

    public RetiredCourse RetiredCourseInfo { get; set; }

    public double AverageRating { get; set; }

    public DateTimeOffset DownloadedOn { get; set; }

    public List<SkillPathHeader> SkillPaths { get; set; }

    public string Byline
    {
        get
        {
            if (Authors == null || Authors.Count <= 0)
            {
                return "";
            }
            if (Authors.Count < 3)
            {
                return string.Join(" and ", Authors.Select((Author c) => c.FullName).ToArray());
            }
            if (Authors.Count == 3)
            {
                return Authors[0].FullName + ", " + Authors[1].FullName + ", and " + Authors[2].FullName;
            }
            return Authors[0].FullName + ", " + Authors[1].FullName + ", and " + (Authors.Count - 2) + " others";
        }
    }

    public CourseDetail()
    {
        Authors = new List<Author>();
        Modules = new List<Module>();
    }

    public int GetClipIndexFromIds(string moduleId, string clipId)
    {
        return Modules.FirstOrDefault((Module m) => m.Name == moduleId)?.Clips.FirstOrDefault((Clip c) => c.Name == clipId)?.Index ?? (-1);
    }

    public override int GetHashCode()
    {
        return base.Name.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is CourseDetail courseDetail)
        {
            return courseDetail.Name == base.Name;
        }
        return false;
    }
}
