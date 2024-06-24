using System.Data;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class CreateClipTranscriptTable : IMigration
{
	public int VersionNumber => 9;

	public void ApplyMigration(DatabaseConnectionManager connectionManager)
	{
		using IDbConnection dbConnection = connectionManager.OpenConnection();
		using IDbCommand dbCommand = dbConnection.CreateCommand();
		dbCommand.CommandText = "CREATE TABLE ClipTranscript (Id integer PRIMARY KEY, StartTime integer, EndTime interger, Text text, ClipId integer REFERENCES Clip(Id) ON DELETE CASCADE);";
		dbCommand.ExecuteNonQuery();
		dbCommand.CommandText = "CREATE INDEX index_ClipTranscriptStart ON ClipTranscript(StartTime)";
		dbCommand.ExecuteNonQuery();
	}
}
