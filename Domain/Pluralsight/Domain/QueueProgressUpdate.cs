namespace Pluralsight.Domain;

public class QueueProgressUpdate
{
	public double Percent { get; set; }

	public int Index { get; set; }

	public int Count { get; set; }

	public CourseDetail Course { get; set; }
}
