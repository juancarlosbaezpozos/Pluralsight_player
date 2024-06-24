using System.Data;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class InitialState : IMigration
{
	public int VersionNumber => 1;

	public void ApplyMigration(DatabaseConnectionManager connectionManager)
	{
		using IDbConnection dbConnection = connectionManager.OpenConnection();
		using (IDbCommand dbCommand = dbConnection.CreateCommand())
		{
			dbCommand.CommandText = "CREATE TABLE Version ( CurrentVersion int );";
			dbCommand.ExecuteNonQuery();
		}
		using IDbCommand dbCommand2 = dbConnection.CreateCommand();
		dbCommand2.CommandText = "CREATE TABLE User ( Jwt text, JwtExpiration text, DeviceId text, RefreshToken text );";
		dbCommand2.ExecuteNonQuery();
	}
}
