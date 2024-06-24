using System.Collections.Generic;

namespace Pluralsight.Domain;

public class SearchResultsDao
{
	public List<CourseHeaderDao> Collection { get; set; }

	public int TotalResults { get; set; }

	public PaginationInfo Pagination { get; set; }
}
