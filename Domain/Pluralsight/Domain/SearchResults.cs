using System.Collections.Generic;

namespace Pluralsight.Domain;

public class SearchResults
{
	public List<SearchHit> Collection { get; set; }

	public int TotalResults { get; set; }

	public PaginationInfo Pagination { get; set; }
}
