using System.Collections.Generic;

namespace Pluralsight.Domain;

public class ModuleProgressDao
{
	public string ModuleId { get; set; }

	public List<string> ViewedClipIds { get; set; }
}
