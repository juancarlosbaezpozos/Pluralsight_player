using System;
using System.Collections.Generic;

namespace Pluralsight.Domain;

public class Module
{
	public List<Clip> Clips { get; set; }

	public string Title { get; set; }

	public string Name { get; set; }

	public string AuthorHandle { get; set; }

	public string Description { get; set; }

	public TimeSpan DurationInMilliseconds { get; set; }

	public Module()
	{
		Clips = new List<Clip>();
	}
}
