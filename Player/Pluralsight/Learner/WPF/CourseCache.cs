using System.Collections.Concurrent;

namespace Pluralsight.Learner.WPF;

public static class CourseCache
{
	public static readonly ConcurrentDictionary<string, double> DownloadProgress = new ConcurrentDictionary<string, double>();
}
