using System;

namespace Pluralsight.Domain;

public class LastViewedClipInfoDao
{
	public string ModuleId { get; set; }

	public string ClipId { get; set; }

	public DateTimeOffset ViewTime { get; set; }
}
