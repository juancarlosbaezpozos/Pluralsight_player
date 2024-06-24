using System.Collections.Generic;

namespace Pluralsight.Domain;

public class CourseIdsDao
{
	public string Id { get; set; }

	public IList<ModuleIdsDao> Modules { get; set; }
}
