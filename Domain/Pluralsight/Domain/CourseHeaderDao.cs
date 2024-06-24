using System;
using System.Collections.Generic;

namespace Pluralsight.Domain;

public class CourseHeaderDao
{
	public string Id { get; set; }

	public string Name { get; set; }

	public string Title { get; set; }

	public DateTimeOffset ReleaseDate { get; set; }

	public DateTimeOffset UpdatedDate { get; set; }

	public string Level { get; set; }

	public List<AuthorHeaderDao> Authors { get; set; }

	public string Color { get; set; }

	public string ImageUrl { get; set; }

	public string DefaultImageUrl { get; set; }

	public string ShortDescription { get; set; }

	public TimeSpan DurationInMilliseconds { get; set; }

	public bool HasTranscript { get; set; }

	public double AverageRating { get; set; }
}
