using System.Net;

namespace Pluralsight.Domain;

public class RestResponse<T>
{
	public T Data { get; set; }

	public HttpStatusCode StatusCode { get; set; }

	public string RawContent { get; set; }
}
