using System.Net;
using System.Threading.Tasks;

namespace Pluralsight.Domain;

public class TermsAndPrivacy
{
	private IRestHelper restHelper;

	public TermsAndPrivacy(IRestHelper restHelper)
	{
		this.restHelper = restHelper;
	}

	public async Task<bool> GetStatus()
	{
		RestResponse<TermsAndPrivacyAccpetedDao> restResponse = await restHelper.AuthenticatedGet<TermsAndPrivacyAccpetedDao>("termsandprivacy/acceptance");
		if (restResponse.StatusCode == HttpStatusCode.OK)
		{
			return restResponse.Data.Accepted;
		}
		return true;
	}

	public async void SetAccepted()
	{
		await restHelper.AuthenticatedPost<object, object>("termsandprivacy/acceptance", null);
	}
}
