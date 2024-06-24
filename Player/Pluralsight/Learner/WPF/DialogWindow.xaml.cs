using System.Windows;
using System.Windows.Markup;

namespace Pluralsight.Learner.WPF;

public partial class DialogWindow : Window, IComponentConnector
{
	private readonly MessageWindowController controller;

	public bool ProgressBarEnabled
	{
		get
		{
			return TheProgressBar.Visibility == Visibility.Visible;
		}
		set
		{
			TheProgressBar.Maximum = 1.0;
			TheProgressBar.Visibility = ((!value) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	public DialogWindow(MessageWindowController controller)
	{
		this.controller = controller;
		base.DataContext = controller;
		InitializeComponent();
		base.Loaded += UpdateWindow_Loaded;
	}

	public void ProgressUpdate(double percent)
	{
		Application.Current.Dispatcher.Invoke(delegate
		{
			TheProgressBar.Value = percent;
		});
	}

	private void UpdateWindow_Loaded(object sender, RoutedEventArgs e)
	{
		controller.OnWindowShown();
	}

	private void Button2Clicked(object sender, RoutedEventArgs e)
	{
		controller.Accepted();
		Close();
	}

	private void CancelClicked(object sender, RoutedEventArgs e)
	{
		controller.Canceled();
		Close();
	}
}
