using System.IO;

namespace Pluralsight.Domain.Persistance;

public interface IDownloadFileLocator
{
	FileInfo GetClipFileInfo(CourseDetail course, Module module, Clip clip);

	string GetFilenameForCourseImage(CourseDetail course);

	string GetFolderForCourseDownloads(CourseDetail course);

	string GetFolderForCoursesDownloads();

	bool IsCourseDownloadComplete(CourseDetail course);

	long CourseSizeOnDisk(CourseDetail course);

	long AvailableFreeSpaceOnDisk();

	bool DownloadLocationExists();
}
