namespace Pluralsight.Domain.WPF.Persistance;

public class AnalyticsDto
{
	public long Id { get; set; }

	public string EventName { get; set; }

	public string Properties { get; set; }

	public string Timestamp { get; set; }

	public int FailureCount { get; set; }

	public string Sent { get; set; }
}
