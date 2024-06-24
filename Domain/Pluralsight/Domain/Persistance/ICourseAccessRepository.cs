namespace Pluralsight.Domain.Persistance;

public interface ICourseAccessRepository
{
	bool? Load(string courseName);

	void Save(string courseName, bool mayDownload);

	void ClearAll();
}
