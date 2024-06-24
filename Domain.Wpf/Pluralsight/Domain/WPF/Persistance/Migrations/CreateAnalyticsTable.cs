using System.Data;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class CreateAnalyticsTable : IMigration
{
    public int VersionNumber => 14;

    public void ApplyMigration(DatabaseConnectionManager connectionManager)
    {
        using IDbConnection dbConnection = connectionManager.OpenConnection();
        using IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE Analytics ( Id integer PRIMARY KEY, EventName text, Properties text, Timestamp text, Sent text, FailureCount integer);";
        dbCommand.ExecuteNonQuery();
        dbCommand.CommandText = "CREATE INDEX index_AnalyticsSent ON Analytics(Sent)";
        dbCommand.ExecuteNonQuery();
    }
}
