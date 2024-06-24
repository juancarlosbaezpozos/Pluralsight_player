using System;

namespace Pluralsight.Domain;

public class LastViewedClipInfo
{
	public string ModuleName { get; set; }

	public string ModuleAuthorHandle { get; set; }

	public int ClipModuleIndex { get; set; }

	public DateTimeOffset ViewTime { get; set; }
}
