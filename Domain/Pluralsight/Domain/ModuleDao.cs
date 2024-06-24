using System;
using System.Collections.Generic;

namespace Pluralsight.Domain;

public class ModuleDao
{
	public string Id { get; set; }

	public string Title { get; set; }

	public string AuthorHandle { get; set; }

	public string Description { get; set; }

	public TimeSpan DurationInMilliseconds { get; set; }

	public IList<ClipDao> Clips { get; set; }
}
