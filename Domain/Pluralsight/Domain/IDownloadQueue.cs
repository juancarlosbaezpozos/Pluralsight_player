using System.Threading.Tasks;

namespace Pluralsight.Domain;

public interface IDownloadQueue
{
    Task QueueCourseForDownload(CourseDetail course);

    void DeleteCourse(CourseDetail course);
}
