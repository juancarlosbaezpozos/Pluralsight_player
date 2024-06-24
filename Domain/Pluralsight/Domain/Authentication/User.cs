namespace Pluralsight.Domain.Authentication;

public class User
{
	public RegisteredDevice DeviceInfo { get; set; }

	public AuthenticationToken AuthToken { get; set; }

	public string UserHandle { get; set; }
}
