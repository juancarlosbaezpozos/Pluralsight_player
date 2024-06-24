using System.Collections.Generic;

namespace Pluralsight.Domain;

public class ModuleProgress
{
	public string ModuleName { get; set; }

	public string ModuleAuthorHandle { get; set; }

	public List<int> ViewedClipIndexes { get; set; }
}
