using System;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using Pluralsight.Domain;
using Pluralsight.Domain.Authentication;

namespace Pluralsight.Learner.WPF;

public partial class FeedbackWindow : Window, IComponentConnector
{
	private AuthenticationToken authToken;

	public FeedbackWindow(AuthenticationToken authToken)
	{
		InitializeComponent();
		UserFeedback.Focus();
		this.authToken = authToken;
	}

	private void CancelClicked(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void SendClicked(object sender, RoutedEventArgs e)
	{
		if (!string.IsNullOrWhiteSpace(UserFeedback.Text))
		{
			Version version = Environment.OSVersion.Version;
			FeedbackDto feedback = new FeedbackDto
			{
				Os = GetOsName(version),
				OsVersion = version.ToString(),
				AppVersion = Assembly.GetEntryAssembly().GetName().Version.ToString(),
				DeviceModel = "",
				Comment = UserFeedback.Text
			};
			new UserFeedback(ObjectFactory.Get<IRestHelper>()).SendFeedback(feedback, authToken);
		}
		Close();
	}

	public static string GetOsName()
	{
		Version version = Environment.OSVersion.Version;
		return GetOsName(version) + $" ({version})";
	}

	private static string GetOsName(Version osVersion)
	{
		string result = "Windows";
		if (osVersion.Major == 6)
		{
			switch (osVersion.Minor)
			{
			case 1:
				result = "Windows 7";
				break;
			case 2:
				result = "Windows 8";
				break;
			case 3:
				result = "Windows 8.1";
				break;
			}
		}
		if (osVersion.Major == 10)
		{
			result = "Windows 10";
		}
		return result;
	}
}
