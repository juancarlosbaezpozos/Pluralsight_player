using System;
using System.Collections.Generic;
using System.Threading;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain;

public class AnalyticsManager
{
    private readonly IAnalyticsRepository analyticsRepository;

    private readonly ITracking tracking;

    private static readonly object analyticsLock = new object();

    private Timer processQueueTimer;

    public bool HasInternetConnection { get; set; }

    public AnalyticsManager(IAnalyticsRepository analyticsRepository, ITracking tracking)
    {
        this.analyticsRepository = analyticsRepository;
        this.tracking = tracking;
    }

    public void KickProcessQueue()
    {
        lock (analyticsLock)
        {
            if (processQueueTimer == null)
            {
                processQueueTimer = new Timer(ProcessQueue, null, TimeSpan.FromSeconds(5.0), Timeout.InfiniteTimeSpan);
            }
        }
    }

    private void ProcessQueue(object state)
    {
        processQueueTimer?.Dispose();
        processQueueTimer = null;
        if (!HasInternetConnection)
        {
            return;
        }
        List<AnalyticsEvent> list = analyticsRepository.LoadQueue();
        if (list.Count == 0)
        {
            return;
        }
        foreach (AnalyticsEvent item in list)
        {
            tracking.SendEvent(item);
            analyticsRepository.AddSentTimestamp(item.Id);
        }
    }
}
