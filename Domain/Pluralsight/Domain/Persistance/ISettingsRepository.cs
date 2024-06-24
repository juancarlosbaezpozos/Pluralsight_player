namespace Pluralsight.Domain.Persistance;

public interface ISettingsRepository
{
	void UpdateApiVersion(string value);

	string GetApiVersion();

	void Save(string name, object value);

	string Load(string name);

	double LoadDouble(string name, double defaultValue = 0.0);

	int LoadInt(string name, int defaultValue = 0);

	bool LoadBool(string name, bool defaultValue = false);

	T LoadEnum<T>(string name, T defaultValue) where T : struct;
}
