using System.Data;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class AlterCourseTableAddImageUrls : IMigration
{
    public int VersionNumber => 7;

    public void ApplyMigration(DatabaseConnectionManager connectionManager)
    {
        using IDbConnection dbConnection = connectionManager.OpenConnection();
        using IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "ALTER TABLE Course ADD COLUMN ImageUrl text";
        dbCommand.ExecuteNonQuery();
        dbCommand.CommandText = "ALTER TABLE Course ADD COLUMN DefaultImageUrl text";
        dbCommand.ExecuteNonQuery();
        dbCommand.CommandText = "ALTER TABLE Course ADD COLUMN IsStale integer";
        dbCommand.ExecuteNonQuery();
        dbCommand.CommandText = "UPDATE Course SET IsStale=1,DefaultImageUrl='https://s.pluralsight.com/learner/a4a714229164486d4b5a40c81559cc6d.jpg'";
        dbCommand.ExecuteNonQuery();
    }
}
