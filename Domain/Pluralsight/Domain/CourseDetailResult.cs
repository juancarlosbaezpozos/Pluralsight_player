namespace Pluralsight.Domain;

public class CourseDetailResult
{
	public CourseDetail Course { get; set; }

	public string ErrorMessage { get; set; }

	public bool Success => string.IsNullOrEmpty(ErrorMessage);
}
