using System;
using System.Diagnostics;
using System.Windows;

namespace Pluralsight.Learner.WPF;

internal class BrowserLauncher
{
	private readonly Window parentWindow;

	public BrowserLauncher(Window parentWindow)
	{
		this.parentWindow = parentWindow;
	}

	public void LaunchUrl(string url)
	{
		try
		{
			Process.Start(url);
		}
		catch (Exception ex)
		{
			DialogWindow dialogWindow = new DialogWindow(new MessageBoxWindowController("An error occurred while attempting to launch a browser to " + url + ". " + ex.Message + ".", "Unable to Launch Browser", null, "OK"));
			dialogWindow.Owner = parentWindow;
			dialogWindow.ShowDialog();
		}
	}
}
