using System;

namespace Pluralsight.Domain.Authentication;

public class SliceInfo
{
	public string DisplayName { get; set; }

	public DateTimeOffset Expiration { get; set; }
}
