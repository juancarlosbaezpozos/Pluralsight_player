using System.Data;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class CreateSettingsTable : IMigration
{
	public int VersionNumber => 10;

	public void ApplyMigration(DatabaseConnectionManager connectionManager)
	{
		using IDbConnection dbConnection = connectionManager.OpenConnection();
		using IDbCommand dbCommand = dbConnection.CreateCommand();
		dbCommand.CommandText = "CREATE TABLE Settings ( Name text PRIMARY KEY, Value text);";
		dbCommand.ExecuteNonQuery();
	}
}
