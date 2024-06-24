using System.Data;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class CreateCourseAccessTable : IMigration
{
	public int VersionNumber => 8;

	public void ApplyMigration(DatabaseConnectionManager connectionManager)
	{
		using IDbConnection dbConnection = connectionManager.OpenConnection();
		using IDbCommand dbCommand = dbConnection.CreateCommand();
		dbCommand.CommandText = "Create Table CourseAccess (Id integer PRIMARY KEY,  CourseName text REFERENCES Course(Name) ON DELETE CASCADE, MayDownload integer, LastChecked text);";
		dbCommand.ExecuteNonQuery();
	}
}
