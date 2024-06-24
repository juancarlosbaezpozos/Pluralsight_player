using System.Collections.Generic;

namespace Pluralsight.Domain;

public class ModuleIdsDao
{
	public string Id { get; set; }

	public IList<string> Clips { get; set; }
}
