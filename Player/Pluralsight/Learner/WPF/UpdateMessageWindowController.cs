using System;
using System.Reflection;
using Pluralsight.Domain;

namespace Pluralsight.Learner.WPF;

internal class UpdateMessageWindowController : MessageWindowController
{
	public UpdateMessageWindowController()
	{
		base.MessageContent = "Checking for updates ...";
		base.CancelButtonText = "Cancel";
		base.Title = "Check for Updates";
	}

	public override async void OnWindowShown()
	{
		Version version = await ObjectFactory.Get<AppUpdater>().CheckForUpdates();
		Version version2 = Assembly.GetEntryAssembly().GetName().Version;
		if (version == null || version <= version2)
		{
			string text = version2.ToString();
			base.MessageContent = "You're up to date! Version " + text + " is the latest version.";
			base.CancelButtonText = "OK";
		}
		else
		{
			base.MessageContent = "Nearly up to date, relaunch the app to finish updating to version " + version;
			base.CancelButtonText = "Relaunch Later";
			base.AcceptButtonText = "Relaunch Now";
		}
	}

	public override void Accepted()
	{
		ObjectFactory.Get<AppUpdater>().RestartToApply();
	}
}
