using System.Net;
using System.Threading.Tasks;

namespace Pluralsight.Domain;

public class Subscription
{
	private IRestHelper restHelper;

	public Subscription(IRestHelper restHelper)
	{
		this.restHelper = restHelper;
	}

	public async Task<UserSubscription> LoadSubscription()
	{
		RestResponse<UserSubscription> restResponse = await restHelper.AuthenticatedGet<UserSubscription>("user/subscription");
		if (restResponse.StatusCode == HttpStatusCode.OK)
		{
			return restResponse.Data;
		}
		return null;
	}
}
