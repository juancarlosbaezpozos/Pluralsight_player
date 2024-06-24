using Pluralsight.Domain.Authentication;

namespace Pluralsight.Domain.Persistance;

public interface IUserRepository
{
	User Load();

	void Save(User user);

	void Delete();
}
