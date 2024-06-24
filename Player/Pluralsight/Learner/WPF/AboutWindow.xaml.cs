using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace Pluralsight.Learner.WPF;

public partial class AboutWindow : Window, IComponentConnector
{
    public AboutWindow()
    {
        InitializeComponent();
        DataContext = new AboutModel();
    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void LaunchBrowser(object sender, RequestNavigateEventArgs e)
    {
        new BrowserLauncher(this).LaunchUrl(e.Uri.AbsoluteUri);
        e.Handled = true;
    }
}
