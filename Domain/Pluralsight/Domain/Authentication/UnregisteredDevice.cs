using System;

namespace Pluralsight.Domain.Authentication;

public class UnregisteredDevice : RegisteredDevice
{
	public string Pin { get; set; }

	public DateTimeOffset ValidUntil { get; set; }

	public DateTimeOffset ServerTime { get; set; }
}
