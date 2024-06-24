namespace Pluralsight.Domain.WPF.Persistance;

internal class ModuleDto
{
	public int Id { get; set; }

	public string Name { get; set; }

	public string Title { get; set; }

	public string AuthorHandle { get; set; }

	public string Description { get; set; }

	public int DurationInMilliseconds { get; set; }

	public int ModuleIndex { get; set; }

	public string CourseName { get; set; }
}
