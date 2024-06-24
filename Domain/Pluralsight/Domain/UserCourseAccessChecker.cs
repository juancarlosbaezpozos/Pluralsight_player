using System.Net;
using System.Threading.Tasks;

namespace Pluralsight.Domain;

public class UserCourseAccessChecker : IUserCourseAccessChecker
{
	private readonly IRestHelper restHelper;

	public UserCourseAccessChecker(IRestHelper restHelper)
	{
		this.restHelper = restHelper;
	}

	public async Task<bool?> MayDownload(string courseName)
	{
		RestResponse<UserCourseAccess> restResponse = await restHelper.AuthenticatedGet<UserCourseAccess>("/user/courses/" + courseName + "/access");
		if (restResponse.StatusCode != HttpStatusCode.OK)
		{
			return null;
		}
		return true;
	}
}
