using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Pluralsight.Domain;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Learner.WPF;

public class CourseTileViewModel : INotifyPropertyChanged
{
	private readonly IDownloadFileLocator downloadFileLocator;

	private readonly DownloadManager downloadManager;

	private readonly CourseProgressFetcher courseProgressFetcher;

	private readonly IUserCourseAccessChecker userCourseAccessChecker;

	private bool downloadLimitReached;

	private readonly ICourseAccessRepository courseAccessRepository;

	private readonly ICourseRepository courseRepository;

	private bool stateLoaded;

	private bool isCourseInDatabase;

	private bool isCourseDownloaded;

	private bool isCourseDownloading;

	private bool isCourseQueued;

	private bool courseFolderExists;

	private bool mayView;

	private bool didDownloadFail;

	public double DownloadProgressAngle { get; set; }

	public string PercentDownloadedText { get; set; }

	public string Title => Course.Title;

	public string Byline => Course.Byline;

	public string Level => Course.Level;

	public string Description => Course.Description;

	public TimeSpan DurationInMilliseconds => Course.DurationInMilliseconds;

	public DateTimeOffset ReleaseDate
	{
		get
		{
			if (!(Course.UpdatedDate != DateTimeOffset.MinValue))
			{
				return Course.ReleaseDate;
			}
			return Course.UpdatedDate;
		}
	}

	public string CourseSize => BytesToReadable(downloadFileLocator.CourseSizeOnDisk(Course));

	public Uri ImageUri
	{
		get
		{
			if (Course.ImageUrl == null)
			{
				return new Uri(Course.DefaultImageUrl);
			}
			return new Uri(Course.ImageUrl);
		}
	}

	public string ImageUrl
	{
		get
		{
			string filenameForCourseImage = downloadFileLocator.GetFilenameForCourseImage(Course);
			if (File.Exists(filenameForCourseImage))
			{
				return filenameForCourseImage;
			}
			return Course.ImageUrl ?? Course.DefaultImageUrl;
		}
	}

	public BitmapImage ImageBitmap
	{
		get
		{
			try
			{
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.UriSource = new Uri(ImageUrl);
				bitmapImage.EndInit();
				return bitmapImage;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}

	public Visibility PlayVisibility
	{
		get
		{
			if (!isCourseDownloaded || !isCourseInDatabase)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public Visibility DownloadingVisibility
	{
		get
		{
			if (!isCourseDownloading)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public Visibility DownloadFailedVisibility
	{
		get
		{
			if (!courseFolderExists || !didDownloadFail)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public Visibility QueuedVisibility
	{
		get
		{
			if (!isCourseQueued)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public Visibility PreDownloadVisibility
	{
		get
		{
			if ((!isCourseDownloaded && !isCourseDownloading && !isCourseQueued && (!didDownloadFail || !courseFolderExists)) || !isCourseInDatabase)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public Visibility IconDownloadVisibility
	{
		get
		{
			if (!StateLoaded)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public Visibility LockVisibility
	{
		get
		{
			if (!mayView)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	private bool StateLoaded
	{
		get
		{
			return stateLoaded;
		}
		set
		{
			stateLoaded = value;
			OnPropertyChanged("DownloadButtonText");
			OnPropertyChanged("IconDownloadVisibility");
		}
	}

	public bool MayView
	{
		get
		{
			return mayView;
		}
		set
		{
			mayView = value;
			OnPropertyChanged("MayView");
			OnPropertyChanged("LockVisibility");
			OnPropertyChanged("MayDownload");
			OnPropertyChanged("DownloadButtonText");
			OnPropertyChanged("IconDownloadVisibility");
		}
	}

	public bool DownloadLimitReached
	{
		set
		{
			downloadLimitReached = value;
			OnPropertyChanged("MayDownload");
			OnPropertyChanged("DownloadButtonText");
			OnPropertyChanged("IconDownloadVisibility");
		}
	}

	public bool MayDownload
	{
		get
		{
			if (mayView)
			{
				return !downloadLimitReached;
			}
			return false;
		}
	}

	public string DownloadButtonText
	{
		get
		{
			if (StateLoaded)
			{
				if (!downloadLimitReached)
				{
					return "Download Course";
				}
				return "Download Limit Reached";
			}
			return "Loading...";
		}
	}

	public string StartCourseString
	{
		get
		{
			if (CourseProgress?.LastViewed == null)
			{
				return "Start course";
			}
			return "Resume course";
		}
	}

	public CourseDetail Course { get; private set; }

	public CourseProgress CourseProgress { get; private set; }

	public double PercentComplete
	{
		get
		{
			if (CourseProgress == null || CourseProgress.ViewedModules.Count == 0)
			{
				return 0.0;
			}
			int num = CourseProgress.ViewedModules.SelectMany((ModuleProgress x) => x.ViewedClipIndexes).Count();
			int num2 = Course.Modules.SelectMany((Module x) => x.Clips).Count();
			return (double)num / (double)num2;
		}
	}

	public Brush ProgressColor
	{
		get
		{
			if (!(PercentComplete >= 1.0))
			{
				return Brushes.White;
			}
			return Application.Current.Resources["completedGreenBrush"] as SolidColorBrush;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public CourseTileViewModel(CourseDetail course, bool downloadLimitReached)
	{
		this.downloadLimitReached = downloadLimitReached;
		Course = course;
		downloadFileLocator = ObjectFactory.Get<IDownloadFileLocator>();
		downloadManager = ObjectFactory.Get<DownloadManager>();
		downloadManager.CourseProgressUpdated.ProgressChanged += UpdateDownloadProgress;
		courseProgressFetcher = ObjectFactory.Get<CourseProgressFetcher>();
		userCourseAccessChecker = ObjectFactory.Get<IUserCourseAccessChecker>();
		courseAccessRepository = ObjectFactory.Get<ICourseAccessRepository>();
		courseRepository = ObjectFactory.Get<ICourseRepository>();
		mayView = false;
	}

	public async Task LoadState()
	{
		if (!StateLoaded)
		{
			UpdateVisibleState();
			await UpdateMayDownload();
			UpdateProgress().ConfigureAwait(continueOnCapturedContext: false);
			StateLoaded = true;
		}
	}

	public async Task UpdateMayDownload()
	{
		bool? cachedMayDownload = courseAccessRepository.Load(Course.Name);
		if (cachedMayDownload.HasValue)
		{
			MayView = cachedMayDownload.Value;
		}
		bool? flag = await userCourseAccessChecker.MayDownload(Course.Name);
		if (!flag.HasValue)
		{
			if (!cachedMayDownload.HasValue && (isCourseDownloaded || isCourseDownloading))
			{
				MayView = true;
			}
			return;
		}
		MayView = flag.Value;
		if (isCourseDownloaded || isCourseDownloading)
		{
			courseAccessRepository.Save(Course.Name, mayView);
		}
	}

	private void UpdateDownloadProgress(object sender, CourseProgressUpdate progress)
	{
		if (progress.Course.Equals(Course))
		{
			CourseCache.DownloadProgress.AddOrUpdate(Course.Name, progress.Percent, (string key, double val) => progress.Percent);
			PercentDownloadedText = $"{(int)Math.Max(0.0, progress.Percent * 100.0)}";
			DownloadProgressAngle = (1.0 - Math.Max(0.0, progress.Percent)) * Math.PI * 2.0;
			UpdateVisibleState();
		}
	}

	public void UpdateDownloadProgress()
	{
		if (CourseCache.DownloadProgress.TryGetValue(Course.Name, out var value))
		{
			PercentDownloadedText = $"{(int)Math.Max(0.0, value * 100.0)}";
			DownloadProgressAngle = (1.0 - Math.Max(0.0, value)) * Math.PI * 2.0;
			UpdateVisibleState();
		}
	}

	public void DownloadQueueCompleted()
	{
		didDownloadFail = !downloadFileLocator.IsCourseDownloadComplete(Course);
		isCourseDownloading = false;
		CourseCache.DownloadProgress.TryRemove(Course.Name, out var _);
		OnPropertyChanged(null);
	}

	public void UpdateVisibleState()
	{
		OnPropertyChanged(null);
		CourseDetail courseDetail = courseRepository.Load(Course.Name);
		isCourseInDatabase = courseDetail != null;
		isCourseDownloaded = downloadFileLocator.IsCourseDownloadComplete(Course);
		isCourseDownloading = !isCourseDownloaded && downloadManager.IsCourseCurrentlyDownloading(Course);
		isCourseQueued = !isCourseDownloaded && !isCourseDownloading && downloadManager.IsCourseQueuedForDownload(Course);
		courseFolderExists = Directory.Exists(downloadFileLocator.GetFolderForCourseDownloads(Course));
		OnPropertyChanged(null);
	}

	public async Task UpdateProgress()
	{
		CourseProgress = await courseProgressFetcher.GetProgressForCourse(Course.Name);
		OnPropertyChanged(null);
	}

	private string BytesToReadable(long bytes)
	{
		if (bytes == 0L)
		{
			return "";
		}
		string[] array = new string[5] { "B", "KB", "MB", "GB", "TB" };
		double num = bytes;
		int num2 = 0;
		while (num >= 1024.0 && num2 < array.Length - 1)
		{
			num2++;
			num /= 1024.0;
		}
		if (!(num > 99.0))
		{
			return $"{num:0.##} {array[num2]}";
		}
		return $"{num:0.#} {array[num2]}";
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public async Task UpdateCourseDetail()
	{
		if (Course != null && string.IsNullOrEmpty(Course.Description))
		{
			CourseDetailResult courseDetailResult = await ObjectFactory.Get<CourseDownloadCommand>().GetCourseDetail(Course.Name);
			if (courseDetailResult.Course != null)
			{
				Course = courseDetailResult.Course;
				UpdateProgress().ConfigureAwait(continueOnCapturedContext: false);
				OnPropertyChanged(null);
			}
		}
	}
}
