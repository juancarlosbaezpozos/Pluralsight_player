using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Pluralsight.Domain;

public class SupportedVersionChecker : ISupportedVersionChecker
{
	private readonly IRestHelper restHelper;

	public SupportedVersionChecker(IRestHelper restHelper)
	{
		this.restHelper = restHelper;
	}

	public async Task<ApiStatus> CheckApiVersionStatus()
	{
		RestResponse<ApiVersions> restResponse = await restHelper.BasicGet<ApiVersions>("../versions");
		if (restResponse.StatusCode == HttpStatusCode.OK)
		{
			if (restResponse.Data.Supported.Contains(2))
			{
				return ApiStatus.Supported;
			}
			if (restResponse.Data.Deprecated.Contains(2))
			{
				return ApiStatus.Deprecated;
			}
			return ApiStatus.NotSupported;
		}
		return ApiStatus.Unknown;
	}
}
