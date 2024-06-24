using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Pluralsight.Domain;
using Pluralsight.Domain.Authentication;

namespace Pluralsight.Learner.WPF;

public partial class LoginWindow : Window, IComponentConnector
{
	private LoginController controller;

	private ITracking tracking;

	public LoginWindow()
	{
		InitializeComponent();
		userNameTextbox.Focus();
		controller = ObjectFactory.Get<LoginController>();
		tracking = ObjectFactory.Get<ITracking>();
		controller.LoginSucceeded += delegate(User x)
		{
			Application.Current.Dispatcher.Invoke(delegate
			{
				LoginSuccess(x);
			});
		};
		controller.LoginFailed += LoginFailed;
		controller.WaitForRegistrationTick += delegate(int x)
		{
			Application.Current.Dispatcher.Invoke(delegate
			{
				UpdateDeviceExpiresTime(x);
			});
		};
		tracking.SetUser(Guid.NewGuid().ToString());
		base.Title = ConfigurationManager.AppSettings["MainWindowTitle"];
	}

	private void LoginFailed(string errorMessage)
	{
		loginButton.IsEnabled = true;
		LoginErrorText.Text = errorMessage;
		LoginErrorBanner.Visibility = Visibility.Visible;
		LoginProgress.Visibility = Visibility.Collapsed;
		passwordBox.Clear();
		tracking.TrackEvent(Event.LoginFailed);
	}

	private void LoginSuccess(User loggedInUser)
	{
		tracking.SetUser(loggedInUser.UserHandle);
		SearchWindow searchWindow = new SearchWindow(loggedInUser);
		searchWindow.Build(displayPreferences: false);
		tracking.TrackEvent(Event.LoginSuccess);
		Application.Current.MainWindow = searchWindow;
		Close();
		searchWindow.Show();
	}

	private void Login(object sender, RoutedEventArgs e)
	{
		LoginErrorBanner.Visibility = Visibility.Collapsed;
		LoginProgress.Visibility = Visibility.Visible;
		loginButton.IsEnabled = false;
		string text = userNameTextbox.Text;
		string password = passwordBox.Password;
		controller.AttemptLogin(text, password);
	}

	protected override void OnClosed(EventArgs e)
	{
		controller.LoginFailed -= LoginFailed;
		controller.LoginSucceeded -= LoginSuccess;
		base.OnClosed(e);
	}

	private void CheckSignInEnabled(object sender, TextChangedEventArgs e)
	{
		EnableLoginButton();
	}

	private void PasswordSignInEnabled(object sender, RoutedEventArgs e)
	{
		EnableLoginButton();
	}

	private void EnableLoginButton()
	{
		loginButton.IsEnabled = !string.IsNullOrEmpty(userNameTextbox.Text) && !string.IsNullOrEmpty(passwordBox.Password);
	}

	private void ForgotPasswordClicked(object sender, RoutedEventArgs e)
	{
		new BrowserLauncher(this).LaunchUrl("https://app.pluralsight.com/id/ForgotPassword");
	}

	private void AuthUrlClicked(object sender, RoutedEventArgs e)
	{
		new BrowserLauncher(this).LaunchUrl("https://pluralsight.com/auth");
	}

	private void NeedHelpClicked(object sender, RoutedEventArgs e)
	{
		new BrowserLauncher(this).LaunchUrl("https://help.pluralsight.com/help/app-signin-sso-apps");
	}

	private async void AlternateSignInClicked(object sender, RoutedEventArgs e)
	{
		string text = await controller.StartDeviceRegistration();
		if (text != null)
		{
			AlternateSignInGrid.Visibility = Visibility.Visible;
			SignInGrid.Visibility = Visibility.Hidden;
			DevicePin.Text = text;
			ChangeAlternateSignIn(pinIsExpired: false);
		}
		else
		{
			DialogWindow dialogWindow = new DialogWindow(new MessageBoxWindowController("Unable to contact server.  Check your internet connection and try again.", "Unable to Contact Server", null, "Cancel"));
			dialogWindow.Owner = this;
			dialogWindow.ShowDialog();
		}
	}

	private void UpdateDeviceExpiresTime(int minutesStillValid)
	{
		if (minutesStillValid >= 0)
		{
			MinutesTillExpires.Text = $"{minutesStillValid} minute" + ((minutesStillValid == 1) ? "" : "s");
		}
		else
		{
			ChangeAlternateSignIn(pinIsExpired: true);
		}
	}

	private void ChangeAlternateSignIn(bool pinIsExpired)
	{
		MinutesTillExpiresMessage.Visibility = (pinIsExpired ? Visibility.Collapsed : Visibility.Visible);
		CodeExpiredMessage.Visibility = ((!pinIsExpired) ? Visibility.Collapsed : Visibility.Visible);
		GenerateNewCodeButton.Visibility = ((!pinIsExpired) ? Visibility.Collapsed : Visibility.Visible);
		DevicePin.Foreground = (pinIsExpired ? Brushes.DimGray : Brushes.White);
	}

	private void CancelAlternateSignIn(object sender, RoutedEventArgs e)
	{
		AlternateSignInGrid.Visibility = Visibility.Hidden;
		SignInGrid.Visibility = Visibility.Visible;
	}

	private void GenerateNewCodeClicked(object sender, RoutedEventArgs e)
	{
		AlternateSignInClicked(null, null);
	}

}
