using System.Net;
using System.Threading.Tasks;

namespace Pluralsight.Domain;

public class Profile
{
	private IRestHelper restHelper;

	public Profile(IRestHelper restHelper)
	{
		this.restHelper = restHelper;
	}

	public async Task<UserProfile> LoadProfile()
	{
		RestResponse<UserProfileDAO> restResponse = await restHelper.AuthenticatedGet<UserProfileDAO>("user/profile");
		if (restResponse.StatusCode == HttpStatusCode.OK)
		{
			return new UserProfile
			{
				Name = restResponse.Data.PersonalData.User.Fullname
			};
		}
		return null;
	}
}
