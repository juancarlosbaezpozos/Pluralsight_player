using System;
using NLog;
using NLog.Config;
using NLog.Targets;
using Pluralsight.Domain;

namespace Pluralsight.Learner.WPF.Logging;

internal class LocalLogger : Pluralsight.Domain.ILogger
{
	private static readonly Logger Logger = GetLogger();

	public void Log(string message, Pluralsight.Domain.LogLevel level)
	{
		Logger.Log(MapLogLevel(level), message);
	}

	private NLog.LogLevel MapLogLevel(Pluralsight.Domain.LogLevel level)
	{
		return level switch
		{
			Pluralsight.Domain.LogLevel.Debug => NLog.LogLevel.Debug, 
			Pluralsight.Domain.LogLevel.Info => NLog.LogLevel.Info, 
			Pluralsight.Domain.LogLevel.Warn => NLog.LogLevel.Warn, 
			Pluralsight.Domain.LogLevel.Error => NLog.LogLevel.Error, 
			_ => NLog.LogLevel.Off, 
		};
	}

	public void LogException(Exception e)
	{
		Logger.Log(NLog.LogLevel.Error, e);
	}

	private static Logger GetLogger()
	{
		LoggingConfiguration loggingConfiguration = new LoggingConfiguration();
		loggingConfiguration.AddRule(target: new FileTarget("logfile")
		{
			FileName = "Pluralsight.exe.log",
			Layout = "${longdate} ${uppercase:${level}} ${message}"
		}, minLevel: NLog.LogLevel.Error, maxLevel: NLog.LogLevel.Error);
		LogManager.Configuration = loggingConfiguration;
		return LogManager.GetLogger("WpfLogger");
	}
}
