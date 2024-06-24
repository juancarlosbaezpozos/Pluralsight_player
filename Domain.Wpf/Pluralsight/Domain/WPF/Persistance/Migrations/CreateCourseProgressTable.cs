using System.Data;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class CreateCourseProgressTable : IMigration
{
	public int VersionNumber => 5;

	public void ApplyMigration(DatabaseConnectionManager connectionManager)
	{
		using IDbConnection dbConnection = connectionManager.OpenConnection();
		using IDbCommand dbCommand = dbConnection.CreateCommand();
		dbCommand.CommandText = "CREATE TABLE CourseProgress ( Name text PRIMARY KEY, ViewedModules text, LastViewedModuleName text, LastViewedModuleAuthor text, LastViewedClip integer, LastViewTime text);";
		dbCommand.ExecuteNonQuery();
	}
}
