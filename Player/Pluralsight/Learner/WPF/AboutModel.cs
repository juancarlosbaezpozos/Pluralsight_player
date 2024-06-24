using System.Reflection;

namespace Pluralsight.Learner.WPF;

internal class AboutModel
{
	public string VersionNumber => Assembly.GetEntryAssembly().GetName().Version.ToString();
}
