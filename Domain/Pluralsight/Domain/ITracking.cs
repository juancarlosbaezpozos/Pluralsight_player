using System;
using System.Collections.Generic;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain;

public interface ITracking
{
	void SetUser(string userIdentifier);

	void SetUser(string userIdentifier, IDictionary<string, object> userProperties);

	void SetCustomAspect(CustomAspect aspect, string value);

	void TrackEvent(Event action);

	void TrackEvent(Event action, IDictionary<string, object> properties);

	void TrackFinalEvent(Event action);

	void TrackException(Exception e);

	void SendEvent(AnalyticsEvent analyticsEvent);
}
