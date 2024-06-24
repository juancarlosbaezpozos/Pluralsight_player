namespace Pluralsight.Domain;

public class RetiredCourse
{
	public bool IsRetired => true;

	public string ReplacementCourseName { get; set; }

	public string ReplacementCourseTitle { get; set; }

	public string RetiredReason { get; set; }
}
