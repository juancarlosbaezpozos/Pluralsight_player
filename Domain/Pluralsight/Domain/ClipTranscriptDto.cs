using System.Collections.Generic;

namespace Pluralsight.Domain;

public class ClipTranscriptDto
{
    public int ModuleIndexPosition { get; set; }

    public List<TranscriptDto> Transcripts { get; set; }
}
