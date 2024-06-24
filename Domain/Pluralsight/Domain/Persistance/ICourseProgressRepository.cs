namespace Pluralsight.Domain.Persistance;

public interface ICourseProgressRepository
{
	CourseProgress Load(string courseName);

	void Save(CourseProgress courseProgress);

	void ClearAll();
}
