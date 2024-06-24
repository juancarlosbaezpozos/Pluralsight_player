namespace Pluralsight.Domain;

public class SkillPathHeader
{
	public string PathId { get; set; }

	public string Title { get; set; }

	public int Version { get; set; }

	public int NumberOfCourses { get; set; }

	public int NumberOfHours { get; set; }

	public string ThumbnailUrl { get; set; }

	public bool IsRetired { get; set; }
}
