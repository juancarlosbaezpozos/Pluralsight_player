using System;

namespace Pluralsight.Domain.Authentication;

public class SubscriptionInfo
{
	public DateTimeOffset? Expiration { get; set; }

	public SliceInfo[] Slices { get; set; }

	public string[] SliceCourseNames { get; set; }
}
