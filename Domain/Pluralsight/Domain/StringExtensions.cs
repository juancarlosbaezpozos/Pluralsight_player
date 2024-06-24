namespace Pluralsight.Domain;

public static class StringExtensions
{
	public static string TruncateAt(this string toTruncate, int truncateAt)
	{
		if (toTruncate.Length > truncateAt)
		{
			return toTruncate.Substring(0, GetTruncateLength(truncateAt)) + "...";
		}
		return toTruncate;
	}

	private static int GetTruncateLength(int truncateAt)
	{
		if (truncateAt > 5)
		{
			return truncateAt - 5;
		}
		return 1;
	}
}
