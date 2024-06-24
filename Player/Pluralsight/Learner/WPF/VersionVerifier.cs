using System.Threading.Tasks;
using System.Windows;
using Pluralsight.Domain;

namespace Pluralsight.Learner.WPF;

public class VersionVerifier
{
	private readonly ISupportedVersionChecker supportedVersionChecker;

	public VersionVerifier(ISupportedVersionChecker supportedVersionChecker)
	{
		this.supportedVersionChecker = supportedVersionChecker;
	}

	public async Task VerifyApiVersion(Window parent)
	{
		ApiStatus num = await supportedVersionChecker.CheckApiVersionStatus();
		if (num == ApiStatus.NotSupported)
		{
			CustomMessageBox.Show(parent, "This version of the application is no longer supported. Update to continue using Pluralsight.", "Player Out Of Date", "OK", null);
		}
		if (num == ApiStatus.Deprecated)
		{
			CustomMessageBox.Show(parent, "A new version of the offline player is available.  Update soon to avoid interruptions.", "Update available", null, "OK");
		}
	}
}
