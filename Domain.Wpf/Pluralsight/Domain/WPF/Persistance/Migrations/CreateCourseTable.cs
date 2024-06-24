using System.Data;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class CreateCourseTable : IMigration
{
	public int VersionNumber => 3;

	public void ApplyMigration(DatabaseConnectionManager connectionManager)
	{
		using IDbConnection dbConnection = connectionManager.OpenConnection();
		using IDbCommand dbCommand = dbConnection.CreateCommand();
		dbCommand.CommandText = "CREATE TABLE Course ( Name text PRIMARY KEY, Title text, ReleaseDate text, UpdatedDate text, Level text, ShortDescription text, Description text, DurationInMilliseconds integer, HasTranscript integer,  AuthorsFullnames text);";
		dbCommand.ExecuteNonQuery();
	}
}
