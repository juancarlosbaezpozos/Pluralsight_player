using System;

namespace Pluralsight.Domain.Authentication;

public class AuthenticationToken
{
	public string Jwt { get; set; }

	public DateTimeOffset Expiration { get; set; }
}
