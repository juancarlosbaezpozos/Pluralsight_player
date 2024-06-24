using System;

namespace Pluralsight.Domain;

public class ClipDao
{
    public string Id { get; set; }

    public string Title { get; set; }

    public int Index { get; set; }

    public TimeSpan DurationInMilliseconds { get; set; }

    public bool SupportsStandard { get; set; }

    public bool SupportsWidescreen { get; set; }
}
