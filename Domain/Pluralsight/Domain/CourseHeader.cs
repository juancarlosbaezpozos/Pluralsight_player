using System;

namespace Pluralsight.Domain;

public class CourseHeader
{
	public string Name { get; set; }

	public string Title { get; set; }

	public DateTimeOffset ReleaseDate { get; set; }

	public DateTimeOffset UpdatedDate { get; set; }

	public string Level { get; set; }

	public string Color { get; set; }

	public string ImageUrl { get; set; }

	public string DefaultImageUrl { get; set; }

	public string UrlSlug { get; set; }
}
