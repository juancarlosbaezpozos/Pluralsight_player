using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BoxedAppSDK;
using Microsoft.Shell;
using Microsoft.Win32;
using Pluralsight.Domain;
using Pluralsight.Domain.Authentication;
using Pluralsight.Domain.Persistance;
using Pluralsight.Domain.WPF.Persistance;

namespace Pluralsight.Learner.WPF;

public partial class App : Application, ISingleInstanceApp
{
    private bool isFirstRunningInstance;

    private bool handlingTokenException;

    private const string AppId = "{da117414-14f4-488c-84a9-e4e645fd49e0}";

    public App()
    {
        SquirrelEvents.HandleEvents();
    }

    private async void App_Start(object sender, StartupEventArgs e)
    {
        isFirstRunningInstance = SingleInstance<App>.InitializeAsFirstInstance("{da117414-14f4-488c-84a9-e4e645fd49e0}");
        if (isFirstRunningInstance)
        {
            await RunApplication();
            await SignalExternalCommandLineArgs(Environment.GetCommandLineArgs());
        }
        else
        {
            Application.Current.Shutdown();
        }
    }

    private MessageBoxResult VerifyCourseLocationExists(ISettingsRepository settingsRepository)
    {
        if (!new DownloadFileLocator().DownloadLocationExists())
        {
            string text = settingsRepository.Load("DownloadLocationRoot");
            return CustomMessageBox.Show(null, "Unable to access (" + text + ")\n\nRestore this disk or change your download location to re-download courses.", "Download location missing", "Continue", "Cancel");
        }
        return MessageBoxResult.OK;
    }

    private async Task RunApplication()
    {
        Application.Current.DispatcherUnhandledException += UnhandledException;
        SquirrelEvents.RegisterApplicationUri();
        BoxedAppSDK.NativeMethods.BoxedAppSDK_SetContext("85ab0e82-0da5-4040-af90-ca0de0be10c4");
        BoxedAppSDK.NativeMethods.BoxedAppSDK_Init();
        new TypeRegistry().RegisterTypes();
        UserRepository userRepository = new UserRepository(ObjectFactory.Get<DatabaseConnectionManager>());
        User user = userRepository.Load();
        await HttpClientFactory.CheckProxySettings();
        MessageBoxResult result = MessageBoxResult.OK;
        ISettingsRepository settingsRepository = ObjectFactory.Get<ISettingsRepository>();
        VersionMigrator versionMigrator = ObjectFactory.Get<VersionMigrator>();
        VideoEncryption.String1V2 = "\0¿{U9\u0001®`ë\u0013Ñ[\u001bÏ";
        VideoEncryption.String2V2 = "\u0002\u008d\a\u0099\u0089\u009a%\u0084K°súÁ48äcz@\u009f,í>ö\u00a02\vß\n@*í\vz\u008c\u0004½\u0093\0ÜeË\u0086\u001f\bÖ\u009e ADÓg&ì¶\u0017\u008dÀ\u0014{µìß\u0088Ø\u009fòÕÄ\u0081pªªtC\u008a@\u009c2:Åf\\\\\u00adè\u009eý\u0002g\u0003|ØBf\u0092\u00a0";
        Window firstWindow;
        if (user != null)
        {
            firstWindow = new SearchWindow(user);
            if (settingsRepository.GetApiVersion() != "v2")
            {
                result = VerifyCourseLocationExists(settingsRepository);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
                await versionMigrator.PerformMigration();
            }
            ((SearchWindow)firstWindow).Build(result == MessageBoxResult.Yes);
        }
        else
        {
            if (settingsRepository.GetApiVersion() != "v2")
            {
                await versionMigrator.PerformMigration();
            }
            firstWindow = new LoginWindow();
        }
        firstWindow.Show();
        ObjectFactory.Get<ITracking>().TrackEvent(Event.AppLaunched);
        await CheckThatApplicationCanRun(firstWindow);
        await ObjectFactory.Get<AppUpdater>().CheckForUpdates();
    }

    private async Task CheckThatApplicationCanRun(Window firstWindow)
    {
        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Active Setup\\Installed Components\\{6BF52A52-394A-11d3-B153-00C04F79FAA6}");
        if ((registryKey == null || (int)registryKey.GetValue("IsInstalled") != 1) && CustomMessageBox.Show(firstWindow, "You'll need to install Window Media Player to play videos", "Windows Media Player Not Installed", "Get Windows Media Player", "OK") == MessageBoxResult.Yes)
        {
            new BrowserLauncher(firstWindow).LaunchUrl("https://support.microsoft.com/en-us/help/14209/get-windows-media-player");
        }
        await new VersionVerifier(ObjectFactory.Get<ISupportedVersionChecker>()).VerifyApiVersion(firstWindow);
    }

    private void UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        if (e.Exception is RefreshTokenRevokedException)
        {
            ForceReAuthenticate();
            e.Handled = true;
            return;
        }
        try
        {
            ObjectFactory.Get<ILogger>().LogException(e.Exception);
            ObjectFactory.Get<ITracking>().TrackException(e.Exception);
        }
        catch
        {
        }
        Application.Current.Shutdown();
    }

    private void ForceReAuthenticate()
    {
        if (!handlingTokenException)
        {
            handlingTokenException = true;
            if (Application.Current.MainWindow is SearchWindow searchWindow)
            {
                CustomMessageBox.Show(searchWindow, "This device is no longer authenticated.  Log in again to access your Pluralsight downloads", "Authorization Required", null, "OK");
                LoginWindow loginWindow = new LoginWindow();
                Application.Current.MainWindow = loginWindow;
                searchWindow.Close();
                loginWindow.Show();
                ClearUserData();
            }
            handlingTokenException = false;
        }
    }

    private void ClearUserData()
    {
        ICourseAccessRepository courseAccessRepository = ObjectFactory.Get<ICourseAccessRepository>();
        IUserRepository userRepository = ObjectFactory.Get<IUserRepository>();
        IUserProfileRepository userProfileRepository = ObjectFactory.Get<IUserProfileRepository>();
        courseAccessRepository.ClearAll();
        userRepository.Delete();
        userProfileRepository.Delete();
    }

    private void App_Exit(object sender, ExitEventArgs e)
    {
        if (isFirstRunningInstance)
        {
            //ObjectFactory.Get<ITracking>().TrackFinalEvent(Event.AppClosed);
            SingleInstance<App>.Cleanup();
        }
    }

    public async Task<bool> SignalExternalCommandLineArgs(IList<string> args)
    {
        Window mainWindow = Application.Current.MainWindow;
        if (mainWindow == null)
        {
            return true;
        }
        if (mainWindow.WindowState == WindowState.Minimized)
        {
            mainWindow.WindowState = WindowState.Normal;
        }
        mainWindow.Activate();
        if (mainWindow is SearchWindow searchWindow && args.Count == 3 && args[1] == "-u")
        {
            await searchWindow.ProcessApplicationUrl(args[2]);
        }
        return true;
    }

    public void InitializeComponent_partial()
    {
        if (!_contentLoaded)
        {
            _contentLoaded = true;
            base.Startup += App_Start;
            base.Exit += App_Exit;
            Uri resourceLocator = new Uri("/Pluralsight;component/Pluralsight/Learner/WPF/app.xaml", UriKind.Relative);
            Application.LoadComponent(this, resourceLocator);
        }
    }

    [STAThread]
    public static void Main()
    {
        var app = new App();
        app.InitializeComponent_partial();
        app.Run();
    }
}
