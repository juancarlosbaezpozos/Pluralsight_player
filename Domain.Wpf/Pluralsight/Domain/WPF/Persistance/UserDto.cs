namespace Pluralsight.Domain.WPF.Persistance;

internal class UserDto
{
	public string Jwt { get; set; }

	public string JwtExpiration { get; set; }

	public string DeviceId { get; set; }

	public string RefreshToken { get; set; }

	public string UserHandle { get; set; }
}
