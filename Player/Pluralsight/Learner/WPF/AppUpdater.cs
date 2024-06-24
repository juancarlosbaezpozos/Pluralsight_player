using System;
using System.Configuration;
using System.Threading.Tasks;
using Pluralsight.Learner.WPF.Logging;
using Squirrel;

namespace Pluralsight.Learner.WPF;

public class AppUpdater
{
	private ReleaseEntry updatedTo;

	public async Task<Version> CheckForUpdates()
	{
		_ = 1;
		try
		{
			string text = ConfigurationManager.AppSettings["SquirrelUpdateUrl"];
			if (!string.IsNullOrEmpty(text))
			{
				using UpdateManager manager = new UpdateManager(text);
				UpdateInfo updateInfo = await manager.CheckForUpdate();
				if (updateInfo != null && updateInfo.ReleasesToApply.Count > 0)
				{
					await manager.UpdateApp();
					updatedTo = updateInfo.FutureReleaseEntry;
				}
			}
		}
		catch (Exception e)
		{
			new LocalLogger().LogException(e);
		}
		return updatedTo?.Version.Version;
	}

	public void RestartToApply()
	{
		UpdateManager.RestartApp();
	}
}
