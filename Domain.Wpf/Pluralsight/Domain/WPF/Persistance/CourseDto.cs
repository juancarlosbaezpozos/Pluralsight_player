namespace Pluralsight.Domain.WPF.Persistance;

internal class CourseDto
{
    public string Name { get; set; }

    public string UrlSlug { get; set; }

    public string Title { get; set; }

    public string ReleaseDate { get; set; }

    public string UpdatedDate { get; set; }

    public string Level { get; set; }

    public string ShortDescription { get; set; }

    public string Description { get; set; }

    public int DurationInMilliseconds { get; set; }

    public int HasTranscript { get; set; }

    public string AuthorsFullNames { get; set; }

    public string ImageUrl { get; set; }

    public string DefaultImageUrl { get; set; }

    public string CachedOn { get; set; }

    public string DownloadedOn { get; set; }
}
