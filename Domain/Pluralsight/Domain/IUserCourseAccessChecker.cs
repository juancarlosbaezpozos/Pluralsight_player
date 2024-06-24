using System.Threading.Tasks;

namespace Pluralsight.Domain;

public interface IUserCourseAccessChecker
{
	Task<bool?> MayDownload(string courseName);
}
