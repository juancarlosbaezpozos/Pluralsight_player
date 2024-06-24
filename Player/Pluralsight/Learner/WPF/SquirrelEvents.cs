using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using Pluralsight.Domain.WPF.Persistance;
using Pluralsight.Learner.WPF.Logging;
using Squirrel;

namespace Pluralsight.Learner.WPF;

internal class SquirrelEvents
{
	public static void HandleEvents()
	{
		SquirrelAwareApp.HandleEvents(OnAppInstall, OnAppUpdate, null, OnAppUnistall);
	}

	private static void OnAppInstall(Version obj)
	{
		try
		{
			using UpdateManager updateManager = GetUpdateManager();
			updateManager?.CreateShortcutForThisExe();
		}
		catch (Exception e)
		{
			new LocalLogger().LogException(e);
			throw;
		}
	}

	private static void OnAppUpdate(Version obj)
	{
		try
		{
			using UpdateManager updateManager = GetUpdateManager();
			updateManager?.CreateShortcutForThisExe();
		}
		catch (Exception e)
		{
			new LocalLogger().LogException(e);
			throw;
		}
	}

	private static void OnAppUnistall(Version version)
	{
		LocalLogger localLogger = new LocalLogger();
		try
		{
			new TypeRegistry().RegisterTypes();
			string folderForCoursesDownloads = new DownloadFileLocator().GetFolderForCoursesDownloads();
			if (!ReleaseVersionInstalled(folderForCoursesDownloads))
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(folderForCoursesDownloads);
				try
				{
					directoryInfo.Delete(recursive: true);
				}
				catch
				{
				}
				new DatabaseConnectionManager().DeleteDatabase();
			}
		}
		catch (Exception e)
		{
			localLogger.LogException(e);
		}
		try
		{
			UnregisterApplicationUri();
			using UpdateManager updateManager = GetUpdateManager();
			updateManager?.RemoveShortcutForThisExe();
		}
		catch (Exception e2)
		{
			localLogger.LogException(e2);
		}
	}

	private static bool ReleaseVersionInstalled(string coursesPath)
	{
		return File.Exists(Path.Combine(coursesPath, "..\\Pluralsight.exe"));
	}

	private static UpdateManager GetUpdateManager()
	{
		//string text = ConfigurationManager.AppSettings["SquirrelUpdateUrl"];
		//if (!string.IsNullOrEmpty(text))
		//{
		//	return new UpdateManager(text);
		//}
		return null;
	}

	public static void RegisterApplicationUri()
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE")?.OpenSubKey("Classes", writable: true);
		RegistryKey registryKey2 = registryKey.CreateSubKey("pluralsight", writable: true);
		RegistryKey registryKey3 = registryKey2.CreateSubKey("shell", writable: true);
		RegistryKey registryKey4 = registryKey3.CreateSubKey("open", writable: true);
		RegistryKey registryKey5 = registryKey4.CreateSubKey("command", writable: true);
		registryKey2.SetValue("URL Protocol", "");
		registryKey5.SetValue("", GetApplicationPath());
		registryKey.Close();
		registryKey2.Close();
		registryKey3.Close();
		registryKey4.Close();
		registryKey5.Close();
	}

	private static void UnregisterApplicationUri()
	{
		try
		{
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE")?.OpenSubKey("Classes", writable: true);
			if (registryKey?.OpenSubKey("pluralsight") != null)
			{
				registryKey.DeleteSubKeyTree("pluralsight");
			}
			registryKey?.Close();
		}
		catch (Exception e)
		{
			new LocalLogger().LogException(e);
		}
	}

	private static string GetApplicationPath()
	{
		using UpdateManager updateManager = GetUpdateManager();
		if (updateManager == null)
		{
			return "\"" + Assembly.GetExecutingAssembly().Location + "\" -u \"%1\"";
		}
		return "\"" + Path.Combine(updateManager.RootAppDirectory, "Pluralsight.exe") + "\" -u \"%1\"";
	}
}
