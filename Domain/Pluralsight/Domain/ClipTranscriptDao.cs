using System.Collections.Generic;

namespace Pluralsight.Domain;

public class ClipTranscriptDao
{
    public string LanguageCode { get; set; }

    public string ClipId { get; set; }

    public List<TranscriptDao> Transcripts { get; set; }
}
