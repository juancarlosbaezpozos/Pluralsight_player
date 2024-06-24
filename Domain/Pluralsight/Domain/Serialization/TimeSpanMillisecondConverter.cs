using System;
using Newtonsoft.Json;

namespace Pluralsight.Domain.Serialization;

public class TimeSpanMillisecondConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
		}
		else
		{
			writer.WriteValue((int)((TimeSpan)value).TotalMilliseconds);
		}
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		try
		{
			return TimeSpan.FromMilliseconds(Convert.ToInt32(reader.Value));
		}
		catch (Exception innerException)
		{
			throw new JsonSerializationException($"Error converting value '{reader.Value}' to TimeSpan", innerException);
		}
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(TimeSpan);
	}
}
