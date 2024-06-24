using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Pluralsight.Domain;
using Pluralsight.Domain.Persistance;
using Segment;
using Segment.Model;

namespace Pluralsight.Learner.WPF.Logging;

internal class SegmentTracking : ITracking
{
	private readonly IAnalyticsRepository analyticsRepository;

	private string userIdentifier;

	private readonly Dictionary<string, object> defaultProperties;

	private static readonly string AssemblyVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();

	public SegmentTracking()
	{
		analyticsRepository = ObjectFactory.Get<IAnalyticsRepository>();
		string text = ConfigurationManager.AppSettings["SegmentSource"];
		if (text != null && text.Contains("Dev"))
		{
			Analytics.Initialize("z29YiNBYMC62BQWGxuzocv9V9pwAwjCD");
		}
		else
		{
			Analytics.Initialize("0Mcpoo91j4Ih8Jii4jLzxUcbToDEVPz9");
		}
		defaultProperties = new Dictionary<string, object> { { "version", AssemblyVersion } };
		Analytics.Client.Succeeded += Client_Succeeded;
		Analytics.Client.Failed += Client_Failed;
	}

	private void Client_Failed(BaseAction action, Exception e)
	{
		if (action.Context.ContainsKey("RowId") && long.TryParse(action.Context["RowId"].ToString(), out var result) && analyticsRepository.IncrementFailureCount(result) > 10)
		{
			analyticsRepository.Delete(result);
		}
	}

	private void Client_Succeeded(BaseAction action)
	{
		if (action.Context.ContainsKey("RowId") && long.TryParse(action.Context["RowId"].ToString(), out var result))
		{
			analyticsRepository.Delete(result);
		}
		ObjectFactory.Get<AnalyticsManager>().KickProcessQueue();
	}

	public void SetUser(string userIdentifier)
	{
		SetUser(userIdentifier, new Dictionary<string, object>());
	}

	public void SetUser(string userIdentifier, IDictionary<string, object> userProperties)
	{
		this.userIdentifier = userIdentifier;
		Analytics.Client.Identify(userIdentifier, userProperties, CreateDefaultOptions());
	}

	public void SetCustomAspect(CustomAspect aspect, string value)
	{
		defaultProperties[aspect.ToString()] = value;
	}

	public void TrackEvent(Event action)
	{
		TrackEvent(action, new Dictionary<string, object>());
	}

	public void TrackEvent(Event action, IDictionary<string, object> properties)
	{
		if (userIdentifier == null)
		{
			return;
		}
		string segmentName = GetSegmentName(action);
		foreach (KeyValuePair<string, object> defaultProperty in defaultProperties)
		{
			if (!properties.ContainsKey(defaultProperty.Key))
			{
				properties[defaultProperty.Key] = defaultProperty.Value;
			}
		}
		analyticsRepository.Insert(new AnalyticsEvent
		{
			EventName = segmentName,
			Properties = properties,
			Timestamp = DateTimeOffset.Now.ToIso8601Format(),
			FailureCount = 0
		});
		ObjectFactory.Get<AnalyticsManager>().KickProcessQueue();
	}

	public void SendEvent(AnalyticsEvent analyticsEvent)
	{
		if (DateTime.TryParse(analyticsEvent.Timestamp, out var result))
		{
			Options options = CreateDefaultOptions();
			options.SetTimestamp(result);
			options.Context["RowId"] = analyticsEvent.Id;
			Analytics.Client.Track(userIdentifier, analyticsEvent.EventName, analyticsEvent.Properties, options);
		}
	}

	public void TrackFinalEvent(Event action)
	{
		TrackEvent(action);
		Analytics.Client.Flush();
		Analytics.Client.Dispose();
	}

	public void TrackException(Exception e)
	{
		Analytics.Client.Track(userIdentifier, "Unhandled Exception", new Dictionary<string, object>
		{
			{ "message", e.Message },
			{ "stack trace", e.StackTrace },
			{
				"inner exception",
				e.InnerException?.Message
			}
		}, CreateDefaultOptions());
	}

	private string GetSegmentName(Event action)
	{
		switch (action)
		{
		case Event.AppLaunched:
			return "Windows App Opened";
		case Event.AppClosed:
			return "App Closed";
		case Event.ClipStarted:
			return "Clip Started";
		case Event.CourseExpandDownload:
		case Event.CourseDownloadUri:
			return "Course Download Started";
		default:
			return action.ToString();
		}
	}

	private Options CreateDefaultOptions()
	{
		Context context = new Context { 
		{
			"app",
			new Dictionary<string, object> { { "version", AssemblyVersion } }
		} };
		Options options = new Options();
		options.SetContext(context);
		options.SetIntegration("All", enabled: true);
		return options;
	}
}
