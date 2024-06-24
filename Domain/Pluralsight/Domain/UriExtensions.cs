using System;
using System.Collections.Generic;
using System.Linq;

namespace Pluralsight.Domain;

public static class UriExtensions
{
	public static Dictionary<string, string> GetQueryParameters(this Uri uri)
	{
		if (string.IsNullOrEmpty(uri.Query))
		{
			return new Dictionary<string, string>();
		}
		return uri.Query.Split(new string[2] { "?", "&" }, StringSplitOptions.RemoveEmptyEntries).ToDictionary((string x) => x.Split('=')[0], (string x) => Unencode(x.Split('=')[1]));
	}

	private static string Unencode(string value)
	{
		return Uri.UnescapeDataString(value).Replace("+", " ");
	}
}
