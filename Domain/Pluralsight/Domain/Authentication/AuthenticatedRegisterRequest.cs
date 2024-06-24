namespace Pluralsight.Domain.Authentication;

public class AuthenticatedRegisterRequest
{
	public string DeviceModel { get; set; }

	public string DeviceName { get; set; }

	public string Username { get; set; }

	public string Password { get; set; }
}
