using Newtonsoft.Json;

namespace Pluralsight.Domain.Serialization;

public static class JsonExtensions
{
	public static bool TryDeserializeObject<T>(string json, out T typeObject)
	{
		try
		{
			typeObject = JsonConvert.DeserializeObject<T>(json);
			return true;
		}
		catch
		{
			typeObject = default(T);
			return false;
		}
	}
}
