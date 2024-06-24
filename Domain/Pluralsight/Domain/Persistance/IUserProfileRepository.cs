namespace Pluralsight.Domain.Persistance;

public interface IUserProfileRepository
{
	UserProfile Load();

	void Save(UserProfile userProfile);

	void Delete();
}
