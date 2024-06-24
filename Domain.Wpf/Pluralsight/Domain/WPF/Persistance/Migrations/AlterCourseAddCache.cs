using System.Data;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class AlterCourseAddCache : IMigration
{
	public int VersionNumber => 13;

	public void ApplyMigration(DatabaseConnectionManager connectionManager)
	{
		using IDbConnection dbConnection = connectionManager.OpenConnection();
		using IDbCommand dbCommand = dbConnection.CreateCommand();
		dbCommand.CommandText = "ALTER TABLE Course ADD COLUMN CachedOn text";
		dbCommand.ExecuteNonQuery();
		dbCommand.CommandText = "ALTER TABLE Course ADD COLUMN UrlSlug text";
		dbCommand.ExecuteNonQuery();
	}
}
