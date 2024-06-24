using System;

namespace Pluralsight.Domain;

public class TranscriptEntry
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Text { get; set; }
}
