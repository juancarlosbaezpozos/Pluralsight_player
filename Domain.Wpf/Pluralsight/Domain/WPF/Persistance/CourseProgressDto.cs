namespace Pluralsight.Domain.WPF.Persistance;

internal class CourseProgressDto
{
    public string Name { get; set; }

    public string ViewedModules { get; set; }

    public string LastViewedModuleName { get; set; }

    public string LastViewedModuleAuthor { get; set; }

    public int LastViewedClip { get; set; }

    public string LastViewTime { get; set; }
}
