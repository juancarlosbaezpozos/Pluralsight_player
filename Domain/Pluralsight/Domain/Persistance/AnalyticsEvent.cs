using System.Collections.Generic;

namespace Pluralsight.Domain.Persistance;

public class AnalyticsEvent
{
	public long Id { get; set; }

	public string EventName { get; set; }

	public Event Event { get; set; }

	public IDictionary<string, object> Properties { get; set; }

	public string Timestamp { get; set; }

	public string Sent { get; set; }

	public int FailureCount { get; set; }
}
