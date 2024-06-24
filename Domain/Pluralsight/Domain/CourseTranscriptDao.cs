using System.Collections.Generic;

namespace Pluralsight.Domain;

public class CourseTranscriptDao
{
    public string CourseId { get; set; }

    public List<ModuleTranscriptDao> Modules { get; set; }
}
