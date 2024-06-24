using System.Data;
using Dapper;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class AlterUserAddUserHandle : IMigration
{
	public int VersionNumber => 11;

	public void ApplyMigration(DatabaseConnectionManager connectionManager)
	{
		using IDbConnection dbConnection = connectionManager.OpenConnection();
		using (IDbCommand dbCommand = dbConnection.CreateCommand())
		{
			dbCommand.CommandText = "ALTER TABLE User ADD COLUMN UserHandle text";
			dbCommand.ExecuteNonQuery();
		}
		string text = dbConnection.ExecuteScalar<string>("Select Jwt from User limit 1");
		if (text != null)
		{
			string handle = JwtUserHandleExtractor.ExtractHandle(text);
			dbConnection.Execute("Update User set UserHandle = @handle", new { handle });
		}
	}
}
