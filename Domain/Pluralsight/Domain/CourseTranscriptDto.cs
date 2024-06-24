using System.Collections.Generic;

namespace Pluralsight.Domain;

public class CourseTranscriptDto
{
	public string CourseName { get; set; }

	public List<ModuleTranscriptDto> Modules { get; set; }
}
