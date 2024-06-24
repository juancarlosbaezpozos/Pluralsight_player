namespace Pluralsight.Domain;

public class CourseProgressUpdate
{
    public CourseDetail Course { get; set; }

    public double Percent { get; set; }
}
