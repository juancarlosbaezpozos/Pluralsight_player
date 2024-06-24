using System.Collections.Generic;

namespace Pluralsight.Domain;

public class ModuleTranscriptDao
{
	public string ModuleId { get; set; }

	public List<ClipTranscriptDao> ClipTranscripts { get; set; }
}
