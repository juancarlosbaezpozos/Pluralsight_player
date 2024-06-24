namespace Pluralsight.Domain.WPF.Persistance;

internal interface IMigration
{
	int VersionNumber { get; }

	void ApplyMigration(DatabaseConnectionManager connectionManager);
}
