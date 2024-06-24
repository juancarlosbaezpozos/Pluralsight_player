using System.Collections.Generic;

namespace Pluralsight.Domain;

public class ModuleTranscriptDto
{
	public string ModuleName { get; set; }

	public List<ClipTranscriptDto> ClipTranscripts { get; set; }
}
