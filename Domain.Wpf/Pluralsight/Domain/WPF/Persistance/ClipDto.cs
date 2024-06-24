namespace Pluralsight.Domain.WPF.Persistance;

internal class ClipDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Title { get; set; }

    public int ClipIndex { get; set; }

    public int DurationInMilliseconds { get; set; }

    public int SupportsStandard { get; set; }

    public int SupportsWidescreen { get; set; }

    public int ModuleId { get; set; }
}
