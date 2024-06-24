using System;
using System.Text;
using Newtonsoft.Json;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

public class JwtUserHandleExtractor
{
	public static string ExtractHandle(string jwt)
	{
		try
		{
			string text = jwt.Split('.')[1];
			text = text.PadRight(text.Length + (4 - text.Length % 4) % 4, '=');
			byte[] bytes = Convert.FromBase64String(text);
			return JsonConvert.DeserializeObject<PluralsightData>(JsonConvert.DeserializeObject<JwtPayload>(Encoding.UTF8.GetString(bytes)).Uid).UserHandle ?? "UnknownUser";
		}
		catch
		{
			return "UnknownUser";
		}
	}
}
