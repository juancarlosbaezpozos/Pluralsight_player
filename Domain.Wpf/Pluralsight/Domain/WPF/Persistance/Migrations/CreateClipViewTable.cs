using System.Data;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class CreateClipViewTable : IMigration
{
	public int VersionNumber => 6;

	public void ApplyMigration(DatabaseConnectionManager connectionManager)
	{
		using IDbConnection dbConnection = connectionManager.OpenConnection();
		using IDbCommand dbCommand = dbConnection.CreateCommand();
		dbCommand.CommandText = "CREATE TABLE ClipView (CourseName text, AuthorHandle text, ModuleName text, ClipIndex integer, StartViewTime text);";
		dbCommand.ExecuteNonQuery();
	}
}
