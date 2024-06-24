namespace Pluralsight.Domain.Authentication;

public class LoginResult
{
	public bool Success { get; set; }

	public string ErrorMessage { get; set; }

	public User User { get; set; }
}
