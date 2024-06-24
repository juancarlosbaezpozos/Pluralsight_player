using System;
using System.Collections.Generic;
using Pluralsight.Domain;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Learner.WPF.Logging;

public class ConsoleTracking : ITracking
{
	public void SetUser(string userIdentifier)
	{
		Console.WriteLine("User Identifier set to: " + userIdentifier);
	}

	public void SetUser(string userIdentifier, IDictionary<string, object> userProperties)
	{
		Console.WriteLine("User Identifier set to: " + userIdentifier);
	}

	public void SetCustomAspect(CustomAspect aspect, string value)
	{
		Console.WriteLine($"Aspect {aspect} set to {value}");
	}

	public void TrackEvent(Event action)
	{
		Console.WriteLine("Event triggered:" + action);
	}

	public void TrackEvent(Event action, IDictionary<string, object> properties)
	{
		Console.Write("Event triggered: " + action.ToString() + " Context:[ ");
		foreach (KeyValuePair<string, object> property in properties)
		{
			Console.Write(property.Key + "=" + property.Value);
		}
		Console.WriteLine("]");
	}

	public void TrackFinalEvent(Event action)
	{
		Console.WriteLine("Session ending event triggered:" + action);
	}

	public void TrackException(Exception e)
	{
		Console.WriteLine("Tracking Exception: " + e.Message);
	}

	public void SendEvent(AnalyticsEvent analyticsEvent)
	{
		TrackEvent(analyticsEvent.Event, analyticsEvent.Properties);
	}
}
