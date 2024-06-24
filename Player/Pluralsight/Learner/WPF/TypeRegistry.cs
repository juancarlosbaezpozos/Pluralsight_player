using System;
using System.Configuration;
using System.Reflection;
using System.Windows;
using Jot;
using Jot.Configuration;
using Jot.Storage;
using Pluralsight.Domain;
using Pluralsight.Domain.Authentication;
using Pluralsight.Domain.Persistance;
using Pluralsight.Domain.WPF.Persistance;
using Pluralsight.Learner.WPF.Logging;

namespace Pluralsight.Learner.WPF;

public class TypeRegistry
{
	private DownloadManager downloadManagerSingleton;

	private DatabaseConnectionManager databaseConnectionSingleton;

	private ITracking tracking;

	private AppUpdater appUpdaterSingleton;

	private Tracker stateTracker;

	private AnalyticsManager analyticsManagerSingleton;

	public void RegisterTypes()
	{
		databaseConnectionSingleton = new DatabaseConnectionManager();
		databaseConnectionSingleton.UpdateDatabase();
		appUpdaterSingleton = new AppUpdater();
		stateTracker = BuildStateTracker();
		ObjectFactory.Register<AppUpdater>(() => appUpdaterSingleton);
		ObjectFactory.Register<DatabaseConnectionManager>(() => databaseConnectionSingleton);
		ObjectFactory.Register<ISettingsRepository>(() => new SettingsRepository(ObjectFactory.Get<DatabaseConnectionManager>()));
		ObjectFactory.Register<IUserRepository>(() => new UserRepository(ObjectFactory.Get<DatabaseConnectionManager>()));
		ObjectFactory.Register<IRestHelper>(() => new RestHelper(ConfigurationManager.AppSettings["baseUri"], ObjectFactory.Get<IUserRepository>(), "user/authorization/", $"PS Windows Offline Player/{Assembly.GetExecutingAssembly().GetName().Version}", ObjectFactory.Get<ILogger>()));
		ObjectFactory.Register<IDownloadFileLocator>(() => new DownloadFileLocator());
		ObjectFactory.Register<ITranscriptRepository>(() => new TranscriptRepository(ObjectFactory.Get<DatabaseConnectionManager>()));
		ObjectFactory.Register<ILogger>(() => new LocalLogger());
		ObjectFactory.Register<CourseNameToIds>(() => new CourseNameToIds(ObjectFactory.Get<IRestHelper>()));
		ObjectFactory.Register<ICourseProgressRepository>(() => new CourseProgressRepository(ObjectFactory.Get<DatabaseConnectionManager>()));
		ObjectFactory.Register<CourseRepository>(() => new CourseRepository(ObjectFactory.Get<DatabaseConnectionManager>()));
		ObjectFactory.Register<CourseProgressFetcher>(() => new CourseProgressFetcher(ObjectFactory.Get<IRestHelper>(), ObjectFactory.Get<ICourseProgressRepository>(), ObjectFactory.Get<ICourseRepository>()));
		ObjectFactory.Register<ICourseAccessRepository>(() => new CourseAccessRepository(ObjectFactory.Get<DatabaseConnectionManager>()));
		ObjectFactory.Register<ICourseRepository>(() => new CourseRepository(ObjectFactory.Get<DatabaseConnectionManager>()));
		ObjectFactory.Register<IAnalyticsRepository>(() => new AnalyticsRepository(ObjectFactory.Get<DatabaseConnectionManager>()));
		tracking = BuildTracker();
		analyticsManagerSingleton = new AnalyticsManager(ObjectFactory.Get<IAnalyticsRepository>(), tracking);
		ObjectFactory.Register<AnalyticsManager>(() => analyticsManagerSingleton);
		ObjectFactory.Register<VersionMigrator>(() => new VersionMigrator(ObjectFactory.Get<ICourseRepository>(), ObjectFactory.Get<CourseNameToIds>(), ObjectFactory.Get<IDownloadFileLocator>(), ObjectFactory.Get<ISettingsRepository>(), ObjectFactory.Get<IClipViewRepository>(), ObjectFactory.Get<ICourseAccessRepository>(), ObjectFactory.Get<ICourseProgressRepository>()));
		ObjectFactory.Register<ILoginHelper>(() => new LoginHelper(ObjectFactory.Get<IRestHelper>()));
		ObjectFactory.Register<LoginController>(() => new LoginController(ObjectFactory.Get<ILoginHelper>(), ObjectFactory.Get<IUserRepository>()));
		ObjectFactory.Register<IClipViewRepository>(() => new ClipViewRepository(ObjectFactory.Get<DatabaseConnectionManager>()));
		ObjectFactory.Register<IUserProfileRepository>(() => new UserProfileRepository(ObjectFactory.Get<DatabaseConnectionManager>()));
		ObjectFactory.Register<SignOutCommand>(() => new SignOutCommand(ObjectFactory.Get<IRestHelper>(), ObjectFactory.Get<IUserRepository>(), ObjectFactory.Get<IClipViewRepository>(), ObjectFactory.Get<IUserProfileRepository>(), ObjectFactory.Get<ICourseProgressRepository>(), ObjectFactory.Get<ICourseRepository>(), ObjectFactory.Get<ICourseAccessRepository>(), ObjectFactory.Get<IDownloadQueue>()));
		ObjectFactory.Register<ITracking>(() => tracking);
		ObjectFactory.Register<Tracker>(() => stateTracker);
		ObjectFactory.Register<IUserCourseAccessChecker>(() => new UserCourseAccessChecker(ObjectFactory.Get<IRestHelper>()));
		ObjectFactory.Register<ISupportedVersionChecker>(() => new SupportedVersionChecker(ObjectFactory.Get<IRestHelper>()));
		downloadManagerSingleton = new DownloadManager(ObjectFactory.Get<IDownloadFileLocator>(), ObjectFactory.Get<IRestHelper>(), ObjectFactory.Get<ITranscriptRepository>(), ObjectFactory.Get<ISettingsRepository>(), ObjectFactory.Get<ICourseRepository>());
		ObjectFactory.Register<CourseDownloadCommand>(() => new CourseDownloadCommand(ObjectFactory.Get<IUserCourseAccessChecker>(), ObjectFactory.Get<IRestHelper>(), ObjectFactory.Get<ICourseRepository>(), downloadManagerSingleton));
		ObjectFactory.Register<DownloadManager>(() => downloadManagerSingleton);
		ObjectFactory.Register<IDownloadQueue>(() => downloadManagerSingleton);
	}

	private static Tracker BuildStateTracker()
	{
		Tracker tracker = new Tracker
		{
			Store = new JsonFileStore(DiskLocations.SettingsLocation())
		};
		tracker.Configure<Window>().Id((Window w) => w.Name).Properties((Window w) => new { w.Height, w.Width, w.Top, w.Left, w.WindowState })
			.PersistOn("SizeChanged")
			.PersistOn("LocationChanged")
			.WhenApplyingProperty(delegate(Window sender, PropertyOperationData args)
			{
				if (args.Property == "Left")
				{
					args.Value = Math.Min(Math.Max(SystemParameters.VirtualScreenLeft, (double)args.Value), SystemParameters.VirtualScreenWidth - sender.Width);
				}
				if (args.Property == "Top")
				{
					args.Value = Math.Min(Math.Max(SystemParameters.VirtualScreenTop, (double)args.Value), SystemParameters.VirtualScreenHeight - sender.Height);
				}
				if (args.Property == "Width")
				{
					args.Value = Math.Min((double)args.Value, SystemParameters.VirtualScreenWidth);
				}
				if (args.Property == "Height")
				{
					args.Value = Math.Min((double)args.Value, SystemParameters.VirtualScreenHeight);
				}
			});
		return tracker;
	}

	private static ITracking BuildTracker()
	{
		if (ConfigurationManager.AppSettings["Analytics"].ToLower() == "console")
		{
			return new ConsoleTracking();
		}
		return new SegmentTracking();
	}
}
