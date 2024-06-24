using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Jot;
using NuGet;
using Pluralsight.Domain;
using Pluralsight.Domain.Authentication;
using Pluralsight.Domain.Persistance;
using Pluralsight.Domain.WPF.Persistance;
using Squirrel;

namespace Pluralsight.Learner.WPF;

public partial class SearchWindow : Window, IComponentConnector, IStyleConnector
{
	public ObservableCollection<CourseTileViewModel> SearchResults;

	private PaginationInfo searchPagination;

	public ObservableCollection<CourseTileViewModel> LoadedCourses;

	private readonly User loggedInUser;

	private UserProfile userProfile;

	private DownloadManager downloadManager;

	public static readonly RoutedUICommand PreferencesCommand = new RoutedUICommand("Preferences", "Preferences", typeof(SearchWindow));

	public static readonly RoutedUICommand ExitCommand = new RoutedUICommand("Exit", "Exit", typeof(SearchWindow), new InputGestureCollection
	{
		new KeyGesture(Key.F4, ModifierKeys.Alt)
	});

	public static readonly RoutedUICommand LogoutCommand = new RoutedUICommand("Sign out", "SignOut", typeof(SearchWindow));

	public static readonly RoutedUICommand HelpCommand = new RoutedUICommand("Get help", "Help", typeof(SearchWindow));

	public static readonly RoutedUICommand ShortcutCommand = new RoutedUICommand("Keyboard shortcuts", "ViewShortcuts", typeof(SearchWindow));

	public static readonly RoutedUICommand GotoPluralsightCommand = new RoutedUICommand("Go to Pluralsight.com", "GoToPluralsight", typeof(SearchWindow));

	public static readonly RoutedUICommand VideoCheckupCommand = new RoutedUICommand("Video checkup", "VideoCheckup", typeof(SearchWindow));

	public static readonly RoutedUICommand GotoAccountCommand = new RoutedUICommand("Account", "Account", typeof(SearchWindow));

	public static readonly RoutedUICommand GotoSubscriptionCommand = new RoutedUICommand("Subscription", "Subscription", typeof(SearchWindow));

	public static readonly RoutedUICommand GotoCommunicationCommand = new RoutedUICommand("Communication preferences", "CommunicationPreferences", typeof(SearchWindow));

	public static readonly RoutedUICommand FeedbackCommand = new RoutedUICommand("Send feedback", "SendFeedback", typeof(SearchWindow));

	public static readonly RoutedUICommand FullScreenCommand = new RoutedUICommand("Full screen", "FullScreen", typeof(SearchWindow), new InputGestureCollection
	{
		new KeyGesture(Key.F11)
	});

	public static readonly RoutedUICommand DeleteCourseCommand = new RoutedUICommand("Remove course", "Delete", typeof(SearchWindow));

	public static readonly RoutedUICommand ToggleHardwareAccelCommand = new RoutedUICommand("Toggle Hardware Acceleration", "ToggleHardwareAccel", typeof(SearchWindow), new InputGestureCollection
	{
		new KeyGesture(Key.Oem3, ModifierKeys.Alt | ModifierKeys.Control)
	});

	private bool isFullScreen;

	private bool isMinimized;

	private WindowState previousWindowState;

	private CourseRepository courseRepo;

	private CourseProgressRepository courseProgressRepository;

	private IRestHelper restHelper;

	private UserClipViewUploader userClipViewUploader;

	private AnalyticsManager analyticsManager;

	private const int MaximumCoursesUserCanDownload = 30;

	private Window shortcutWindow;

	private ITracking tracking;

	private bool isSearching;

	private string searchTerm;

	private DialogWindow progressWindow;

	private bool showPreferences;

	private TranscriptDownloader transcriptDownloader;

	private NetworkConnectivityManager manager;

	private BrowserLauncher browserLauncher;

	private ISettingsRepository settingsRepository;

	private string SelectedSortOrder
	{
		get
		{
			string key = ((searchOrder.SelectedValue as ComboBoxItem).Content as string).ToLower();
			return new Dictionary<string, string>
			{
				{ "newest", "published_date" },
				{ "relevance", "relevance" }
			}[key];
		}
	}

	public SearchWindow(User loggedInUser)
	{
		base.Title = ConfigurationManager.AppSettings["MainWindowTitle"];
		this.loggedInUser = loggedInUser;
	}

	private async void SearchWindow_StateChanged(object sender, EventArgs e)
	{
		if (isMinimized)
		{
			ObjectFactory.Get<ITracking>().TrackEvent(Event.AppLaunched);
			await CheckTermsAndPrivacyStatus();
		}
		isMinimized = base.WindowState == WindowState.Minimized;
	}

	private async Task CheckTermsAndPrivacyStatus()
	{
		if (!(await new TermsAndPrivacy(restHelper).GetStatus()))
		{
			TermsAndPrivacyCheckBox.IsChecked = false;
			ShowTermsToAccept.Visibility = Visibility.Visible;
			HomeControl.Visibility = Visibility.Collapsed;
		}
	}

	public async void Build(bool displayPreferences)
	{
		settingsRepository = ObjectFactory.Get<ISettingsRepository>();
		showPreferences = displayPreferences;
		tracking = ObjectFactory.Get<ITracking>();
		tracking.SetUser(loggedInUser.UserHandle);
		CourseDownloadPlayRow.loggedInUser = loggedInUser;
		restHelper = ObjectFactory.Get<IRestHelper>();
		transcriptDownloader = new TranscriptDownloader(restHelper, ObjectFactory.Get<ITranscriptRepository>(), settingsRepository, ObjectFactory.Get<ICourseRepository>());
		downloadManager = ObjectFactory.Get<DownloadManager>();
		downloadManager.CourseProgressUpdated.ProgressChanged += DownloadManagerCourseProgressUpdated;
		downloadManager.QueueProgressUpdated.ProgressChanged += UpdateProgressBar;
		downloadManager.FirstClipDownloadFailed += FirstClipDownloadFailed;
		DatabaseConnectionManager connectionManager = ObjectFactory.Get<DatabaseConnectionManager>();
		courseRepo = new CourseRepository(connectionManager);
		courseProgressRepository = new CourseProgressRepository(connectionManager);
		ClipViewRepository clipViewRepository = new ClipViewRepository(connectionManager);
		userClipViewUploader = new UserClipViewUploader(restHelper, clipViewRepository, courseRepo);
		InitializeComponent();
		LoadProfile();
		LoadSubscription();
		SetupLocalLibrary();
		analyticsManager = ObjectFactory.Get<AnalyticsManager>();
		SearchResults = new ObservableCollection<CourseTileViewModel>();
		lvSearchResults.ItemsSource = SearchResults;
		base.Title = ConfigurationManager.AppSettings["MainWindowTitle"];
		manager = new NetworkConnectivityManager();
		manager.OnConnectivityChanged += ConnectionChanged;
		manager.ForceCheckNow();
		Tracker tracker = ObjectFactory.Get<Tracker>();
		base.SourceInitialized += delegate
		{
			tracker.Track(this);
		};
		browserLauncher = new BrowserLauncher(this);
		base.ContentRendered += SearchWindow_ContentRendered;
		base.StateChanged += SearchWindow_StateChanged;
		await CheckTermsAndPrivacyStatus();
		LoadLanguageCodes();
	}

	private void LoadLanguageCodes()
	{
		LanguagesMenuItem.Items.Clear();
		string text = settingsRepository.Load("CaptionsLanguageCode") ?? "en";
		foreach (KeyValuePair<string, string> code in Iso6391Languages.Codes)
		{
			MenuItem menuItem = new MenuItem
			{
				Header = code.Key,
				Name = code.Value,
				IsCheckable = true,
				IsChecked = (text == code.Value)
			};
			menuItem.Click += LanguagesMenuItem_Click;
			LanguagesMenuItem.Items.Add(menuItem);
		}
	}

	private async void LanguagesMenuItem_Click(object sender, RoutedEventArgs e)
	{
		if (sender is MenuItem menuItem)
		{
			SortMenuItem.GetAllChildren().ToList().ForEach(UncheckMenuItem);
			menuItem.IsChecked = true;
			settingsRepository.Save("CaptionsLanguageCode", menuItem.Name);
			LoadLanguageCodes();
			await Task.Run(() => transcriptDownloader.UpdateDownloadedCourseTranscripts());
		}
	}

	private void FirstClipDownloadFailed()
	{
		string msg = "We were unable to download this course.";
		string title = "Download Failed";
		Application.Current.Dispatcher.Invoke(delegate
		{
			if (DownloadManager.CanShowFirstClipDownloadFailedWindow)
			{
				DownloadManager.CanShowFirstClipDownloadFailedWindow = false;
				if (CustomMessageBox.Show(Window.GetWindow(this), msg, title, "Video Check", "Close") == MessageBoxResult.Yes)
				{
					browserLauncher.LaunchUrl("http://app.pluralsight.com/video/test");
				}
			}
		});
	}

	private async void SearchWindow_ContentRendered(object sender, EventArgs e)
	{
		if (showPreferences)
		{
			ShowPreferences();
		}
		await QueueCoursesForDownloadThatArentDoneDownloading();
	}

	private void SetupLocalLibrary()
	{
		Task.Run((Func<Task>)ReloadCourses);
	}

	~SearchWindow()
	{
		manager.Dispose();
	}

	private async void ConnectionChanged(bool hasInternetConnection)
	{
		analyticsManager.HasInternetConnection = hasInternetConnection;
		if (hasInternetConnection)
		{
			await HttpClientFactory.CheckProxySettings();
			analyticsManager.KickProcessQueue();
		}
		await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
		{
			if (hasInternetConnection)
			{
				if (settingsRepository.GetApiVersion() != "v2")
				{
					CustomMessageBox.Show(Window.GetWindow(this), "We need to make some quick updates to your downloaded data.", "Update data", null, "OK");
					UpdateManager.RestartApp();
				}
			}
			else
			{
				CloseAddCourseWindow();
			}
			AddCourseButton.IsEnabled = hasInternetConnection;
			OfflineMessage.Visibility = (hasInternetConnection ? Visibility.Collapsed : Visibility.Visible);
			VideoPlayer.HasInternetConnection = hasInternetConnection;
		});
	}

	private async void LoadSubscription()
	{
		UserSubscription userSubscription = await new Subscription(restHelper).LoadSubscription();
		if (userSubscription == null)
		{
			return;
		}
		bool flag = false;
		if (userSubscription.LibrarySubscription == null || userSubscription.LibrarySubscription.IsExpired)
		{
			if (userSubscription.Slices == null)
			{
				flag = true;
			}
			else if (!userSubscription.Slices.Any((Slice slice) => slice.Expires >= DateTimeOffset.UtcNow))
			{
				flag = true;
			}
		}
		ExpiredMessage.Visibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
		tracking.SetUser(loggedInUser.UserHandle, new Dictionary<string, object>
		{
			{
				"Organization Name",
				userSubscription.LibrarySubscription?.OrgName
			},
			{
				"Subscription Type",
				userSubscription.LibrarySubscription?.OrgType
			},
			{
				"Active Slice",
				userSubscription.IsActiveSliceSubscription()
			},
			{
				"OS",
				FeedbackWindow.GetOsName()
			},
			{ "Platform", "Windows" }
		});
	}

	private async void LoadProfile()
	{
		DatabaseConnectionManager connectionManager = ObjectFactory.Get<DatabaseConnectionManager>();
		UserProfileRepository userProfileRepository = new UserProfileRepository(connectionManager);
		userProfile = userProfileRepository.Load();
		if (userProfile != null)
		{
			ProfileName.Text = userProfile.Name;
			ProfileEmail.Text = userProfile.Email;
		}
		else
		{
			userProfile = await new Profile(restHelper).LoadProfile();
			if (userProfile == null)
			{
				ProfileName.Text = "";
				ProfileEmail.Text = "";
			}
			else
			{
				ProfileName.Text = userProfile.Name;
				ProfileEmail.Text = userProfile.Email;
				userProfileRepository.Save(userProfile);
			}
		}
		userClipViewUploader.UploadAllUnsavedClipViews(loggedInUser.AuthToken);
	}

	private void UpdateProgressBar(object sender, QueueProgressUpdate progress)
	{
		if (DownloadProgress == null)
		{
			return;
		}
		DownloadProgress.Value = progress.Percent;
		if (progress.Percent < 1.0)
		{
			DownloadInfo.Visibility = Visibility.Collapsed;
			ProgressInfo.Visibility = Visibility.Visible;
			CurrentlyDownloadingCourse.Text = progress.Course.Title.TruncateAt(40);
		}
		else
		{
			ProgressInfo.Visibility = Visibility.Collapsed;
			if (IsDownloadLimitReached())
			{
				DownloadInfo.Visibility = Visibility.Visible;
			}
			MarkUnfinishedCoursesAsFailed();
		}
		if (progress.Count > 1)
		{
			QueuedCourseCount.Text = $"({progress.Index} of {progress.Count})";
		}
		else
		{
			QueuedCourseCount.Text = "";
		}
	}

	private void MarkUnfinishedCoursesAsFailed()
	{
		foreach (CourseTileViewModel loadedCourse in LoadedCourses)
		{
			loadedCourse.DownloadQueueCompleted();
		}
	}

	public async Task ProcessApplicationUrl(string url)
	{
		if (url.Contains("&title="))
		{
			url = url.Substring(0, url.IndexOf("&title=", StringComparison.Ordinal));
		}
		Uri uri = new Uri(url);
		if (!(uri.Host == "download"))
		{
			return;
		}
		Dictionary<string, string> queryParameters = uri.GetQueryParameters();
		if (!queryParameters.ContainsKey("id") || (queryParameters.ContainsKey("type") && queryParameters["type"] != "course"))
		{
			return;
		}
		if (IsDownloadLimitReached())
		{
			CustomMessageBox.Show(this, "Download limit reached", "Unable To Download Course", null, "OK");
			return;
		}
		CourseDownloadCommand downloadCommand = ObjectFactory.Get<CourseDownloadCommand>();
		CourseDetailResult courseDetailResult = await downloadCommand.GetCourseDetail(queryParameters["id"]);
		if (courseDetailResult.Success)
		{
			downloadCommand.SaveForDownload(courseDetailResult.Course);
			await ReloadCourseUi();
			tracking.TrackEvent(Event.CourseDownloadUri, new Dictionary<string, object> { { "Referrer", "Web" } });
			await downloadCommand.Download(courseDetailResult.Course);
		}
		else
		{
			CustomMessageBox.Show(this, courseDetailResult.ErrorMessage, "Unable To Download Course", null, "OK");
		}
	}

	private void DownloadManagerCourseProgressUpdated(object sender, CourseProgressUpdate courseProgressUpdate)
	{
	}

	private void AddCourseButtonClick(object sender, RoutedEventArgs e)
	{
		if (AddCourseGrid.Visibility != 0)
		{
			tracking.TrackEvent(Event.SearchExpanded);
			AddCourseButton.Background = Brushes.Black;
			AddCourseGrid.Visibility = Visibility.Visible;
			AddCoursesColumnDefinition.Width = new GridLength(1.0, GridUnitType.Star);
			AddCoursesColumnDefinition.MinWidth = 300.0;
			SearchTextBox.Focus();
		}
		else
		{
			tracking.TrackEvent(Event.SearchCollapsed);
			CloseAddCourseWindow();
		}
	}

	private void CloseAddCourseWindow()
	{
		AddCourseButton.Background = Application.Current.Resources["midGreyBrush"] as Brush;
		AddCourseGrid.Visibility = Visibility.Collapsed;
		AddCoursesColumnDefinition.Width = new GridLength(0.0);
		AddCoursesColumnDefinition.MinWidth = 0.0;
		CourseDetailPopup.IsOpen = false;
	}

	private async void SearchTextBox_OnKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return && !string.IsNullOrEmpty(SearchTextBox.Text))
		{
			searchTerm = SearchTextBox.Text;
			SearchResults.Clear();
			SearchTermTextBlock.Text = "\"" + SearchTextBox.Text + "\"";
			SearchResultsEmptyNotification.Visibility = Visibility.Collapsed;
			SearchHintTextBlock.Visibility = Visibility.Collapsed;
			SearchProgressRing.Visibility = Visibility.Visible;
			searchResultArea.Visibility = Visibility.Collapsed;
			await PerformSearch(searchTerm);
		}
	}

	private async Task PerformSearch(string term)
	{
		string sortOrder = SelectedSortOrder;
		int num = await Task.Run(() => Search(term, sortOrder, 1));
		switch (num)
		{
		case -1:
			return;
		case 0:
			SearchResults.Clear();
			break;
		}
		tracking.TrackEvent(Event.SearchSubmitted);
		SearchProgressRing.Visibility = Visibility.Collapsed;
		SearchResultsEmptyNotification.Visibility = ((num != 0) ? Visibility.Collapsed : Visibility.Visible);
		searchResultArea.Visibility = ((num == 0) ? Visibility.Collapsed : Visibility.Visible);
		SearchHintTextBlock.Visibility = Visibility.Collapsed;
	}

	private async void SearchOrder_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!string.IsNullOrEmpty(searchTerm))
		{
			SearchResultsEmptyNotification.Visibility = Visibility.Collapsed;
			SearchHintTextBlock.Visibility = Visibility.Collapsed;
			SearchProgressRing.Visibility = Visibility.Visible;
			SearchResults.Clear();
			await PerformSearch(searchTerm);
		}
	}

	private void ClearSearch(object sender, RoutedEventArgs e)
	{
		SearchTextBox.Text = "";
		SearchResults.Clear();
		SearchHintTextBlock.Visibility = Visibility.Visible;
		SearchProgressRing.Visibility = Visibility.Collapsed;
		searchResultArea.Visibility = Visibility.Collapsed;
		SearchResultsEmptyNotification.Visibility = Visibility.Collapsed;
	}

	private void CanExit(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = true;
	}

	private void Exit(object sender, ExecutedRoutedEventArgs e)
	{
		Application.Current.Shutdown();
	}

	private void CanLogOut(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = true;
	}

	private void LogOut(object sender, ExecutedRoutedEventArgs e)
	{
		if (CustomMessageBox.Show(this, "Signing out will remove all local content.  This means you will need to download courses again before you will be able to watch. Continue?", "Confirm Sign Out") == MessageBoxResult.Yes)
		{
			tracking.TrackFinalEvent(Event.Logout);
			LoginWindow loginWindow = new LoginWindow();
			Application.Current.MainWindow = loginWindow;
			Close();
			downloadManager.ClearQueue();
			ObjectFactory.Get<SignOutCommand>().Execute(loggedInUser);
			loginWindow.Show();
		}
	}

	private void HelpCmd(object sender, ExecutedRoutedEventArgs e)
	{
		browserLauncher.LaunchUrl("https://help.pluralsight.com/help/how-do-i-view-a-downloaded-course-in-the-macos-app");
	}

	private void GotoPluralsightCmd(object sender, ExecutedRoutedEventArgs e)
	{
		browserLauncher.LaunchUrl("http://app.pluralsight.com");
	}

	private void VideoCheckupCmd(object sender, ExecutedRoutedEventArgs e)
	{
		browserLauncher.LaunchUrl("http://app.pluralsight.com/video/test");
	}

	private void GotoAccountCmd(object sender, ExecutedRoutedEventArgs e)
	{
		browserLauncher.LaunchUrl("http://app.pluralsight.com/mobile-api/redirects/accountsettings");
	}

	private void GotoSubscriptionCmd(object sender, ExecutedRoutedEventArgs e)
	{
		browserLauncher.LaunchUrl("http://app.pluralsight.com/mobile-api/redirects/subscriptionmanagement");
	}

	private void GotoCommunicationCmd(object sender, ExecutedRoutedEventArgs e)
	{
		browserLauncher.LaunchUrl("http://app.pluralsight.com/mobile-api/redirects/communicationpreferences");
	}

	private void FeedbackCmd(object sender, ExecutedRoutedEventArgs e)
	{
		FeedbackWindow feedbackWindow = new FeedbackWindow(loggedInUser.AuthToken);
		feedbackWindow.Owner = Window.GetWindow(this);
		feedbackWindow.WindowStyle = WindowStyle.SingleBorderWindow;
		feedbackWindow.ResizeMode = ResizeMode.NoResize;
		feedbackWindow.ShowDialog();
	}

	private void DeleteCourse(object sender, ExecutedRoutedEventArgs e)
	{
		CourseTileViewModel selected = LoadedCoursesPanel.SelectedItem as CourseTileViewModel;
		if (selected == null || CustomMessageBox.Show(this, "Are you sure you wish to remove this course from your downloads?", "Remove course") != MessageBoxResult.Yes)
		{
			return;
		}
		tracking.TrackEvent(Event.RemoveCourse);
		Task.Run(delegate
		{
			downloadManager.DeleteCourse(selected.Course);
			courseRepo.Delete(selected.Course.Name);
		});
		LoadedCourses.Remove(selected);
		UpdateSearchResultsForCourse(selected.Course.Name, delegate(CourseTileViewModel x)
		{
			x.UpdateVisibleState();
		});
		DisplayLoadedCourses();
		if (SearchResults == null)
		{
			return;
		}
		bool downloadLimitReached = IsDownloadLimitReached();
		foreach (CourseTileViewModel searchResult in SearchResults)
		{
			searchResult.DownloadLimitReached = downloadLimitReached;
		}
	}

	private void ToggleFullScreen(object sender, ExecutedRoutedEventArgs e)
	{
		if (isFullScreen)
		{
			base.Topmost = false;
			base.WindowState = previousWindowState;
			base.WindowStyle = WindowStyle.SingleBorderWindow;
			MenuBar.Visibility = Visibility.Visible;
			isFullScreen = false;
			VideoPlayer.WindowSetToWindowed();
			tracking.TrackEvent(Event.ExitFullScreen);
		}
		else
		{
			previousWindowState = base.WindowState;
			base.Visibility = Visibility.Hidden;
			base.WindowState = WindowState.Normal;
			base.WindowStyle = WindowStyle.None;
			base.WindowState = WindowState.Maximized;
			base.Topmost = true;
			MenuBar.Visibility = Visibility.Collapsed;
			base.Visibility = Visibility.Visible;
			isFullScreen = true;
			VideoPlayer.WindowSetToFullScreen();
			tracking.TrackEvent(Event.PlayerFullScreen);
		}
	}

	private void UpdateSearchResultsForCourse(string courseName, Action<CourseTileViewModel> updateAction)
	{
		IEnumerable<CourseTileViewModel> enumerable = SearchResults?.Where((CourseTileViewModel c) => c.Course.Name == courseName);
		if (enumerable == null)
		{
			return;
		}
		foreach (CourseTileViewModel item in enumerable)
		{
			updateAction(item);
		}
	}

	private void ShowAboutWindow(object sender, RoutedEventArgs e)
	{
		AboutWindow aboutWindow = new AboutWindow();
		aboutWindow.Owner = Window.GetWindow(this);
		aboutWindow.ResizeMode = ResizeMode.NoResize;
		aboutWindow.ShowDialog();
	}

	private async Task ReloadCourses()
	{
		LoadAndSortCourses();
		Application.Current.Dispatcher.Invoke(delegate
		{
			LoadedCoursesPanel.ItemsSource = LoadedCourses;
			DisplayLoadedCourses();
			if (SearchResults != null)
			{
				foreach (CourseTileViewModel searchResult in SearchResults)
				{
					searchResult.DownloadLimitReached = IsDownloadLimitReached();
				}
			}
		});
		List<Task> list = new List<Task>();
		foreach (CourseTileViewModel loadedCourse in LoadedCourses)
		{
			loadedCourse.UpdateDownloadProgress();
			list.Add(loadedCourse.UpdateProgress());
			list.Add(loadedCourse.LoadState());
		}
		await Task.WhenAll(list);
	}

	private void LoadAndSortCourses()
	{
		List<CourseDetail> list = courseRepo.LoadAllDownloaded();
		bool isDownloadLimitReached = list.Count >= 30;
		LoadedCourses = new ObservableCollection<CourseTileViewModel>(list.Select((CourseDetail c) => new CourseTileViewModel(c, isDownloadLimitReached)).ToList());
		string sortCoursesBy = settingsRepository?.Load("SortCoursesBy") ?? "";
		if (!string.IsNullOrWhiteSpace(sortCoursesBy))
		{
			foreach (object item in (IEnumerable)SortMenuItem.Items)
			{
				MenuItem menuItem = item as MenuItem;
				if (menuItem != null)
				{
					Application.Current.Dispatcher.Invoke(() => menuItem.IsChecked = menuItem.Name == sortCoursesBy);
				}
			}
		}
		switch (sortCoursesBy)
		{
		case "DownloadDateSort":
			LoadedCourses = new ObservableCollection<CourseTileViewModel>(LoadedCourses.OrderByDescending((CourseTileViewModel c) => c.Course.DownloadedOn));
			break;
		case "ReleasedDateSort":
			LoadedCourses = new ObservableCollection<CourseTileViewModel>(LoadedCourses.OrderByDescending((CourseTileViewModel c) => c.ReleaseDate));
			break;
		case "RecentlyViewedSort":
		{
			List<CourseProgress> allCoursesProgress = LoadedCourses.Select((CourseTileViewModel c) => courseProgressRepository.Load(c.Course.Name)).ToList();
			List<CourseTileViewModel> list3 = LoadedCourses.ToList();
			list3.Sort(delegate(CourseTileViewModel x, CourseTileViewModel y)
			{
				CourseProgress courseProgress = allCoursesProgress.FirstOrDefault((CourseProgress p) => p?.CourseName == x.Course.Name);
				CourseProgress courseProgress2 = allCoursesProgress.FirstOrDefault((CourseProgress p) => p?.CourseName == y.Course.Name);
				if (courseProgress == null && courseProgress2 == null)
				{
					return 0;
				}
				if (courseProgress == null)
				{
					return 1;
				}
				return (courseProgress2 == null) ? (-1) : DateTimeOffset.Compare(courseProgress2.LastViewed.ViewTime, courseProgress.LastViewed.ViewTime);
			});
			LoadedCourses = new ObservableCollection<CourseTileViewModel>(list3);
			break;
		}
		case "TitleSort":
			LoadedCourses = new ObservableCollection<CourseTileViewModel>(LoadedCourses.OrderBy((CourseTileViewModel c) => c.Title));
			break;
		case "SkillLevelSort":
		{
			List<CourseTileViewModel> list2 = LoadedCourses.ToList();
			list2.Sort(delegate(CourseTileViewModel x, CourseTileViewModel y)
			{
				if (x.Level == y.Level)
				{
					return 0;
				}
				if (x.Level == "Beginner")
				{
					return -1;
				}
				if (y.Level == "Beginner")
				{
					return 1;
				}
				return (x.Level == "Intermediate") ? (-1) : ((y.Level == "Intermediate") ? 1 : 0);
			});
			LoadedCourses = new ObservableCollection<CourseTileViewModel>(list2);
			break;
		}
		case "ProgressSort":
		{
			IOrderedEnumerable<CourseTileViewModel> collection = LoadedCourses.OrderByDescending(delegate(CourseTileViewModel c)
			{
				c.UpdateProgress().ConfigureAwait(continueOnCapturedContext: false);
				return c.PercentComplete;
			});
			LoadedCourses = new ObservableCollection<CourseTileViewModel>(collection);
			break;
		}
		}
	}

	private bool IsDownloadLimitReached()
	{
		return LoadedCourses.Count >= 30;
	}

	private void DisplayLoadedCourses()
	{
		LoadedCoursesPanel.Visibility = ((LoadedCourses.Count <= 0) ? Visibility.Collapsed : Visibility.Visible);
		NoCoursesHint.Visibility = ((LoadedCourses.Count > 0) ? Visibility.Collapsed : Visibility.Visible);
		if (LoadedCourses.Count == 0 && AddCourseGrid.Visibility != 0)
		{
			AddCourseButtonClick(null, null);
		}
		if (IsDownloadLimitReached() && ProgressInfo.Visibility != 0)
		{
			DownloadInfo.Visibility = Visibility.Visible;
		}
		else
		{
			DownloadInfo.Visibility = Visibility.Collapsed;
		}
	}

	private async Task QueueCoursesForDownloadThatArentDoneDownloading()
	{
		DownloadFileLocator downloadFileLocator = new DownloadFileLocator();
		if (!downloadFileLocator.DownloadLocationExists())
		{
			string text = settingsRepository.Load("DownloadLocationRoot");
			CustomMessageBox.Show(Window.GetWindow(this), "Your download location (" + text + ") is invalid", "Disk Problem", null, "OK");
			ShowPreferences();
			await ReloadCourses();
		}
		List<CourseDetail> coursesInDb = courseRepo.LoadAllDownloaded();
		CourseDownloadCommand downloadCommand = ObjectFactory.Get<CourseDownloadCommand>();
		foreach (string courseName in downloadFileLocator.GetCoursesInDownloadsFolder())
		{
			if (!coursesInDb.Exists((CourseDetail c) => c.Name == courseName))
			{
				CourseDetailResult courseDetailResult = await downloadCommand.GetCourseDetail(courseName);
				if (courseDetailResult.Success)
				{
					downloadCommand.SaveForDownload(courseDetailResult.Course);
				}
			}
		}
		List<Task> tasks = new List<Task>();
		for (int num = coursesInDb.Count - 1; num >= 0; num--)
		{
			CourseDetail course = coursesInDb[num];
			if (Directory.Exists(downloadFileLocator.GetFolderForCourseDownloads(course)) && !downloadFileLocator.IsCourseDownloadComplete(course))
			{
				tasks.Add(Task.Run(() => downloadManager.QueueCourseForDownload(course)));
			}
		}
		await ReloadCourseUi();
		await Task.WhenAll(tasks);
	}

	private void DownloadedCoursesKeyPressed(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Delete)
		{
			DeleteCourse(null, null);
		}
	}

	private void SearchResultSelected(object sender, SelectionChangedEventArgs e)
	{
		if (lvSearchResults.SelectedItem == null)
		{
			return;
		}
		Timer spinTimerWaitingForMouseReleased = new Timer
		{
			Interval = 10.0
		};
		spinTimerWaitingForMouseReleased.Elapsed += delegate
		{
			Application.Current.Dispatcher.Invoke(delegate
			{
				if (Mouse.LeftButton == MouseButtonState.Released && Mouse.RightButton == MouseButtonState.Released)
				{
					spinTimerWaitingForMouseReleased.Stop();
					DescriptionScrollViewer.ScrollToTop();
					CourseDetailPopup.IsOpen = true;
				}
			});
		};
		spinTimerWaitingForMouseReleased.Start();
		CourseTileViewModel course = lvSearchResults.SelectedItem as CourseTileViewModel;
		Task.Run(delegate
		{
			course?.LoadState();
			course?.UpdateCourseDetail();
		});
		CourseDetailPopup.DataContext = course;
		tracking.TrackEvent(Event.SearchCourseResultSelected);
	}

	private void SearchListMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (sender is ListBoxItem && !CourseDetailPopup.IsOpen)
		{
			SearchResultSelected(null, null);
		}
	}

	private async void CourseQueuedForDownload()
	{
		tracking.TrackEvent(Event.CourseExpandDownload, new Dictionary<string, object> { { "Referrer", "None" } });
		await ReloadCourseUi();
	}

	private async Task ReloadCourseUi()
	{
		await ReloadCourses();
		if (!IsDownloadLimitReached() || SearchResults == null)
		{
			return;
		}
		foreach (CourseTileViewModel searchResult in SearchResults)
		{
			searchResult.DownloadLimitReached = true;
		}
	}

	private void CourseStartingToPlay(CourseDetail course, CourseProgress courseProgress)
	{
		if (course == null)
		{
			return;
		}
		VideoPlayer.Visibility = Visibility.Visible;
		HomeControl.Visibility = Visibility.Collapsed;
		LastViewedClipInfo lastViewed = courseProgress?.LastViewed;
		if (lastViewed == null)
		{
			VideoPlayer.LaunchCourse(course, course.Modules[0], course.Modules[0].Clips[0]);
			tracking.TrackEvent(Event.CourseStarted, new Dictionary<string, object>
			{
				{ "Screen", "Home" },
				{ "Course ID", course.Name },
				{ "Course Title", course.Title }
			});
		}
		else
		{
			tracking.TrackEvent(Event.CourseResumed, new Dictionary<string, object>
			{
				{ "Screen", "Home" },
				{ "Course ID", course.Name },
				{ "Course Title", course.Title }
			});
			try
			{
				Module module = course.Modules.First((Module x) => x.Name == lastViewed.ModuleName);
				VideoPlayer.LaunchCourse(course, module, module.Clips[lastViewed.ClipModuleIndex]);
			}
			catch
			{
				VideoPlayer.LaunchCourse(course, course.Modules[0], course.Modules[0].Clips[0]);
			}
		}
		CourseDetailPopup.IsOpen = false;
	}

	private void ViewMoreOnPluralsight(object sender, RoutedEventArgs e)
	{
		tracking.TrackEvent(Event.CourseExpandRedirect);
		CourseTileViewModel course = (sender as FrameworkContentElement)?.DataContext as CourseTileViewModel;
		LaunchBrowserToCourse(course);
	}

	private void LaunchBrowserToCourse(CourseTileViewModel course)
	{
		if (course != null)
		{
			string url = "https://app.pluralsight.com/library/courses/" + course.Course.UrlSlug;
			browserLauncher.LaunchUrl(url);
		}
	}

	private void ViewCourseDetails(object sender, RoutedEventArgs e)
	{
		tracking.TrackEvent(Event.ViewCourseDetails);
		CourseTileViewModel course = LoadedCoursesPanel.SelectedItem as CourseTileViewModel;
		LaunchBrowserToCourse(course);
	}

	private void ShowShortcuts(object sender, ExecutedRoutedEventArgs e)
	{
		if (shortcutWindow == null)
		{
			shortcutWindow = new ShortcutWindow();
			shortcutWindow.Closed += delegate
			{
				shortcutWindow = null;
			};
		}
		shortcutWindow.Show();
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		userClipViewUploader.UploadAllUnsavedClipViews(loggedInUser.AuthToken);
		shortcutWindow?.Close();
		base.OnClosing(e);
	}

	private void SearchScrolled(object sender, ScrollChangedEventArgs e)
	{
		double extentHeight = e.ExtentHeight;
		if (e.ViewportHeight + e.VerticalOffset > extentHeight - 5.0 && searchPagination?.Page < searchPagination?.TotalPages && !isSearching)
		{
			isSearching = true;
			Console.WriteLine("Searching for more results");
			string termWhenStarted = searchTerm;
			string sortOrder = SelectedSortOrder;
			PaginationInfo paginationInfo = searchPagination;
			int pageNumber = ((paginationInfo == null) ? 1 : (paginationInfo.Page + 1));
			Task.Run(() => Search(termWhenStarted, sortOrder, pageNumber));
		}
	}

	private int Search(string termWhenStarted, string sortOrder, int pageNumber)
	{
		Search search = new Search(restHelper);
		Task<SearchResults> x = search.For(termWhenStarted, sortOrder, pageNumber);
		int num = 0;
		List<CourseTileViewModel> tileViewModels = new List<CourseTileViewModel>();
		bool downloadLimitReached = IsDownloadLimitReached();
		List<CourseTileViewModel> collection = x.Result.Collection.Select((SearchHit c) => new CourseTileViewModel(c.Course, downloadLimitReached)).ToList();
		tileViewModels.AddRange(collection);
		num = tileViewModels.Count;
		if (searchTerm != termWhenStarted || x.Result == null)
		{
			isSearching = false;
			if (!(searchTerm != termWhenStarted))
			{
				return 0;
			}
			return -1;
		}
		Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
		{
			if (pageNumber <= 1 || SearchResults.Count != 0)
			{
				SearchResults.AddRange(tileViewModels);
				resultCountLabel.Content = $"{x.Result.TotalResults} Results";
				searchPagination = x.Result.Pagination;
				isSearching = false;
			}
		});
		return num;
	}

	private void CheckForUpdates(object sender, RoutedEventArgs e)
	{
		DialogWindow dialogWindow = new DialogWindow(new UpdateMessageWindowController());
		dialogWindow.Owner = Window.GetWindow(this);
		dialogWindow.WindowStyle = WindowStyle.SingleBorderWindow;
		dialogWindow.ResizeMode = ResizeMode.NoResize;
		dialogWindow.ShowDialog();
	}

	private void RenewSubscription(object sender, RoutedEventArgs e)
	{
		browserLauncher.LaunchUrl("https://app.pluralsight.com/id/subscription");
	}

	private async void RefreshSubscription(object sender, RoutedEventArgs e)
	{
		foreach (CourseTileViewModel loadedCourse in LoadedCourses)
		{
			await loadedCourse.UpdateMayDownload();
		}
		LoadSubscription();
	}

	private void EscapeShouldLeaveFullscreen(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			ExitFullscreen();
		}
	}

	private void ExitFullscreen()
	{
		if (isFullScreen)
		{
			ToggleFullScreen(null, null);
		}
	}

	private void WindowLoaded(object sender, RoutedEventArgs e)
	{
		Task.Run(delegate
		{
			if (settingsRepository.LoadBool("SoftwareOnlyRender"))
			{
				Application.Current.Dispatcher.Invoke(delegate
				{
					SetRenderMode(RenderMode.SoftwareOnly);
				});
			}
		});
	}

	private void ToggleHardwareAcceleration(object sender, ExecutedRoutedEventArgs e)
	{
		bool flag = !settingsRepository.LoadBool("SoftwareOnlyRender");
		settingsRepository.Save("SoftwareOnlyRender", flag);
		if (flag)
		{
			CustomMessageBox.Show(this, "Hardware acceleration disabled", "Hardware Acceleration Disabled", null, "OK");
			SetRenderMode(RenderMode.SoftwareOnly);
		}
		else
		{
			CustomMessageBox.Show(this, "Hardware acceleration enabled", "Hardware Acceleration Enabled", null, "OK");
			SetRenderMode(RenderMode.Default);
		}
	}

	private void SetRenderMode(RenderMode preferred)
	{
		if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
		{
			hwndSource.CompositionTarget.RenderMode = preferred;
		}
	}

	private void ShowPreferences(object sender, ExecutedRoutedEventArgs e)
	{
		ShowPreferences();
	}

	public void ShowPreferences()
	{
		PreferencesWindow preferencesWindow = new PreferencesWindow();
		preferencesWindow.Owner = Window.GetWindow(this);
		preferencesWindow.WindowStyle = WindowStyle.SingleBorderWindow;
		preferencesWindow.ResizeMode = ResizeMode.NoResize;
		preferencesWindow.DownloadLocationChanged += OnDownloadLocationChanged;
		preferencesWindow.ShowDialog();
	}

	private void OnDownloadLocationChanged(string originalLocation, string newLocation)
	{
		originalLocation = SettingsNames.GetCoursesDownloadLocation(originalLocation);
		newLocation = SettingsNames.GetCoursesDownloadLocation(newLocation);
		if (VideoPlayer.Visibility != Visibility.Collapsed)
		{
			VideoPlayer.StopPlayingAndCloseThePlayerWindow();
		}
		DirectoryInfo source = new DirectoryInfo(originalLocation);
		DirectoryInfo destination = new DirectoryInfo(newLocation);
		CoursesMover mover = new CoursesMover();
		progressWindow = new DialogWindow(new ProgressMessageWindowController("Moving Downloaded Courses", "Moving Downloads"))
		{
			Owner = Window.GetWindow(this),
			WindowStyle = WindowStyle.None,
			ResizeMode = ResizeMode.NoResize,
			ProgressBarEnabled = true
		};
		mover.ProgressPercent += progressWindow.ProgressUpdate;
		mover.Complete += Mover_Complete;
		Task.Run(delegate
		{
			mover.MoveAll(source, destination);
		});
		progressWindow.ShowDialog();
	}

	private async void Mover_Complete()
	{
		await Application.Current.Dispatcher.Invoke((Func<Task>)async delegate
		{
			progressWindow?.Close();
			await ReloadCourses();
		});
	}

	private void TermsOfUse(object sender, RoutedEventArgs e)
	{
		browserLauncher.LaunchUrl("http://app.pluralsight.com/mobile-api/redirects/termsofservice");
	}

	private void Privacy(object sender, RoutedEventArgs e)
	{
		browserLauncher.LaunchUrl("http://app.pluralsight.com/mobile-api/redirects/privacypolicy");
	}

	private void Continue_Clicked(object sender, RoutedEventArgs e)
	{
		ShowTermsToAccept.Visibility = Visibility.Collapsed;
		HomeControl.Visibility = Visibility.Visible;
		new TermsAndPrivacy(restHelper).SetAccepted();
	}

	public void TermsAndPrivacyCheckBox_OnChecked(object sender, RoutedEventArgs e)
	{
		ContinueButton.IsEnabled = TermsAndPrivacyCheckBox.IsChecked.GetValueOrDefault();
	}

	private void CheckMenuItemByName(DependencyObject obj, string checkedItemName)
	{
		if (obj is MenuItem menuItem)
		{
			menuItem.IsChecked = menuItem.Name == checkedItemName;
		}
	}

	private void UncheckMenuItem(DependencyObject obj)
	{
		if (obj is MenuItem menuItem)
		{
			menuItem.IsChecked = false;
		}
	}

	private async void Sortby_Clicked(object sender, RoutedEventArgs e)
	{
		if (sender is MenuItem menuItem)
		{
			SortMenuItem.GetAllChildren().ToList().ForEach(UncheckMenuItem);
			menuItem.IsChecked = true;
			settingsRepository.Save("SortCoursesBy", menuItem.Name);
			await ReloadCourseUi();
		}
	}

	private async void VideoPlayer_OnSessionCompleted(object sender, RoutedEventArgs e)
	{
		VideoPlayer videoPlayer = e.Source as VideoPlayer;
		ExitFullscreen();
		await LoadedCourses.Single((CourseTileViewModel c) => c.Course.Name == videoPlayer?.Course?.Name).UpdateProgress();
		UpdateSearchResultsForCourse(videoPlayer?.Course?.Name, async delegate(CourseTileViewModel x)
		{
			await x.UpdateProgress();
		});
		VideoPlayer.Visibility = Visibility.Collapsed;
		HomeControl.Visibility = Visibility.Visible;
		userClipViewUploader.UploadAllUnsavedClipViews(loggedInUser.AuthToken);
		await ReloadCourses();
	}

}
