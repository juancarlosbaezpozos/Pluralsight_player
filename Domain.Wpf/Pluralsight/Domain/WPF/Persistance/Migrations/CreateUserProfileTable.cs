using System.Data;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class CreateUserProfileTable : IMigration
{
	public int VersionNumber => 2;

	public void ApplyMigration(DatabaseConnectionManager connectionManager)
	{
		using IDbConnection dbConnection = connectionManager.OpenConnection();
		using IDbCommand dbCommand = dbConnection.CreateCommand();
		dbCommand.CommandText = "CREATE TABLE UserProfile ( Name text, Email text );";
		dbCommand.ExecuteNonQuery();
	}
}
