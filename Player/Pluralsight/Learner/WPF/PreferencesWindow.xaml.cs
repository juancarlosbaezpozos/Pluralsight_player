using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using Pluralsight.Domain;
using Pluralsight.Domain.Persistance;
using Pluralsight.Domain.WPF.Persistance;

namespace Pluralsight.Learner.WPF;

public partial class PreferencesWindow : Window, IComponentConnector
{
	private readonly ISettingsRepository settingsRepository;

	private ITracking tracking;

	private AutoplaySetting originalAutoplayValue;

	private string originalLocation;

	private DownloadManager downloadManager;

	public event Action<string, string> DownloadLocationChanged;

	public PreferencesWindow()
	{
		settingsRepository = ObjectFactory.Get<ISettingsRepository>();
		downloadManager = ObjectFactory.Get<DownloadManager>();
		tracking = ObjectFactory.Get<ITracking>();
		InitializeComponent();
		LoadSettings();
	}

	private void LoadSettings()
	{
		bool flag = settingsRepository.LoadBool("SoftwareOnlyRender");
		HardwareAcceleration.IsChecked = !flag;
		originalAutoplayValue = settingsRepository.LoadEnum("Autoplay", AutoplaySetting.Course);
		Autoplay.SelectedValue = originalAutoplayValue;
		originalLocation = settingsRepository.Load("DownloadLocationRoot");
		if (originalLocation == null)
		{
			DownloadLocation.Text = DiskLocations.DefaultDownloadLocationRoot();
			DownloadLocation.IsReadOnly = true;
			DownloadLocationReset.Visibility = Visibility.Collapsed;
		}
		else
		{
			DownloadLocation.Text = originalLocation;
			DownloadLocationReset.Visibility = Visibility.Visible;
		}
		DownloadLocation.IsEnabled = !downloadManager.ClipsDownloading;
		DownloadLocationReset.IsEnabled = !downloadManager.ClipsDownloading;
		DownloadLocationPathSelect.IsEnabled = !downloadManager.ClipsDownloading;
	}

	private void CancelClicked(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void SaveClicked(object sender, RoutedEventArgs e)
	{
		bool valueOrDefault = HardwareAcceleration.IsChecked.GetValueOrDefault();
		settingsRepository.Save("SoftwareOnlyRender", !valueOrDefault);
		if (Enum.TryParse<AutoplaySetting>(Autoplay.SelectedValue.ToString(), out var result) && originalAutoplayValue != result)
		{
			settingsRepository.Save("Autoplay", result);
			tracking.SetCustomAspect(CustomAspect.Autoplay, result.ToString());
		}
		string text = null;
		if (!DownloadLocation.IsReadOnly && Directory.Exists(DownloadLocation.Text))
		{
			text = DownloadLocation.Text;
		}
		bool flag = (string.IsNullOrEmpty(originalLocation) && !string.IsNullOrWhiteSpace(text)) || (!string.IsNullOrWhiteSpace(originalLocation) && string.IsNullOrWhiteSpace(text));
		if (!string.IsNullOrEmpty(originalLocation) && !string.IsNullOrEmpty(text))
		{
			flag = !originalLocation.Equals(text, StringComparison.CurrentCultureIgnoreCase);
		}
		settingsRepository.Save("DownloadLocationRoot", text);
		Close();
		if (flag)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				text = DiskLocations.DefaultDownloadLocationRoot();
			}
			if (string.IsNullOrWhiteSpace(originalLocation))
			{
				originalLocation = DiskLocations.DefaultDownloadLocationRoot();
			}
			this.DownloadLocationChanged?.Invoke(originalLocation, text);
		}
	}

	private void DownloadLocationPathSelectorClicked(object sender, RoutedEventArgs e)
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
		{
			DownloadLocation.Text = folderBrowserDialog.SelectedPath;
			DownloadLocation.IsReadOnly = false;
			DownloadLocationReset.Visibility = Visibility.Visible;
		}
	}

	private void DownloadLocationReset_Click(object sender, RoutedEventArgs e)
	{
		DownloadLocation.Text = DiskLocations.DefaultDownloadLocationRoot();
		DownloadLocation.IsReadOnly = true;
		DownloadLocationReset.Visibility = Visibility.Collapsed;
	}

	private void DownloadLocation_OnLostFocus(object sender, RoutedEventArgs e)
	{
		if (!Directory.Exists(DownloadLocation.Text))
		{
			CustomMessageBox.Show(this, "Please select a valid directory", "Error", null, "OK");
		}
	}
}
