using System.Threading.Tasks;

namespace Pluralsight.Domain;

public interface IRestHelper
{
	Task<RestResponse<T>> BasicGet<T>(string resource);

	Task<RestResponse<T>> AuthenticatedGet<T>(string resource);

	Task<RestResponse<T>> AuthenticatedDelete<T>(string resource);

	Task<RestResponse<TResp>> BasicPost<TResp, TReq>(string resource, TReq request);

	Task<RestResponse<TResp>> AuthenticatedPost<TResp, TReq>(string resource, TReq request);
}
