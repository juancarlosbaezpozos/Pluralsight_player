using System.IO;

namespace Pluralsight.Domain.Persistance;

public class SettingsNames
{
	public const string PlaybackSpeed = "PlaybackSpeed";

	public const string TableOfContentsOpen = "TocOpen";

	public const string PlaybackVolume = "PlaybackVolume";

	public const string CloseCaptions = "CloseCaption";

	public const string Autoplay = "Autoplay";

	public const string SoftwareOnlyRender = "SoftwareOnlyRender";

	public const string DownloadLocationRoot = "DownloadLocationRoot";

	public const string SortCoursesBy = "SortCoursesBy";

	public const string CaptionsLanguageCode = "CaptionsLanguageCode";

	public static string GetCoursesDownloadLocation(string folder)
	{
		return Path.Combine(folder, "courses");
	}
}
