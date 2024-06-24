using System;

namespace Pluralsight.Domain.Authentication;

public class DeviceAuthorizationResponse
{
	public string Token { get; set; }

	public string UserHandle { get; set; }

	public DateTimeOffset Expiration { get; set; }

	public bool Authenticated { get; set; }

	public DeviceOfflineAccess OfflineAccess { get; set; }
}
