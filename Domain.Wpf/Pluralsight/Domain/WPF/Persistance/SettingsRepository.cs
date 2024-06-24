using System;
using System.Collections.Generic;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain.WPF.Persistance;

public class SettingsRepository : ISettingsRepository
{
    private readonly DatabaseConnectionManager connectionManager;

    private static string api_version;

    private static Dictionary<string, string> Cache = new Dictionary<string, string>();

    public SettingsRepository(DatabaseConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public string GetApiVersion()
    {
        if (api_version == null)
        {
            api_version = Load("APIVersion") ?? "v1";
        }
        return api_version;
    }

    public void UpdateApiVersion(string value)
    {
        Save("APIVersion", value);
        api_version = value;
    }

    public void Save(string name, object value)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        var anon = new
        {
            name = name,
            value = value?.ToString()
        };
        Cache[anon.name] = anon.value;
        if (dbConnectionTs.Execute("UPDATE Settings SET Value=@Value WHERE Name=@Name", anon) == 0)
        {
            dbConnectionTs.Execute("INSERT INTO Settings (Name, Value) VALUES (@Name, @Value)", anon);
        }
    }

    public string Load(string name)
    {
        if (Cache.ContainsKey(name))
        {
            return Cache[name];
        }
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        string text = dbConnectionTs.QueryFirstOrDefault<string>("SELECT Value FROM Settings WHERE Name=@Name LIMIT 1", new
        {
            Name = name
        });
        Cache[name] = text;
        return text;
    }

    public double LoadDouble(string name, double defaultValue = 0.0)
    {
        string text = Load(name);
        if (text == null)
        {
            return defaultValue;
        }
        if (!double.TryParse(text, out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public int LoadInt(string name, int defaultValue = 0)
    {
        string text = Load(name);
        if (text == null)
        {
            return defaultValue;
        }
        if (!int.TryParse(text, out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public T LoadEnum<T>(string name, T defaultValue) where T : struct
    {
        if (!Enum.TryParse<T>(Load(name), out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public bool LoadBool(string name, bool defaultValue = false)
    {
        string text = Load(name);
        if (text == null)
        {
            return defaultValue;
        }
        if (!bool.TryParse(text, out var result))
        {
            return defaultValue;
        }
        return result;
    }
}
