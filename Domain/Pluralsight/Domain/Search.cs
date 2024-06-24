using System.Net;
using System.Threading.Tasks;

namespace Pluralsight.Domain;

public class Search
{
	private IRestHelper restHelper;

	public Search(IRestHelper restHelper)
	{
		this.restHelper = restHelper;
	}

	public async Task<SearchResults> For(string searchTerm, string sort)
	{
		string searchUrl = "library/search/courses?q=" + WebUtility.UrlEncode(searchTerm) + "&sort=" + sort;
		return await ExecuteSearch(searchUrl);
	}

	private async Task<SearchResults> ExecuteSearch(string searchUrl)
	{
		RestResponse<SearchResultsDao> restResponse = await restHelper.BasicGet<SearchResultsDao>(searchUrl);
		if (restResponse.StatusCode == HttpStatusCode.OK)
		{
			return CourseMapper.ToSearchResult(restResponse.Data);
		}
		return null;
	}

	public async Task<SearchResults> For(string searchTerm, string sort, int pageNumber)
	{
		string searchUrl = $"library/search/courses?q={WebUtility.UrlEncode(searchTerm)}&sort={sort}&page={pageNumber}";
		return await ExecuteSearch(searchUrl);
	}
}
