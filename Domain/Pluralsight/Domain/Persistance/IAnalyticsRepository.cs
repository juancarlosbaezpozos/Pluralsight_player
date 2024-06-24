using System.Collections.Generic;

namespace Pluralsight.Domain.Persistance;

public interface IAnalyticsRepository
{
	long Insert(AnalyticsEvent analyticsEvent);

	void Delete(long rowid);

	List<AnalyticsEvent> LoadQueue();

	void AddSentTimestamp(long id);

	int IncrementFailureCount(long id);
}
