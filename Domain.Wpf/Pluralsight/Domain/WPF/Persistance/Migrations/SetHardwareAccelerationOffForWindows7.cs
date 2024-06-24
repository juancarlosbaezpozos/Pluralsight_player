using System;
using System.Data;
using Dapper;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class SetHardwareAccelerationOffForWindows7 : IMigration
{
    public int VersionNumber => 12;

    public void ApplyMigration(DatabaseConnectionManager connectionManager)
    {
        if (IsWindows7(Environment.OSVersion.Version))
        {
            Save(connectionManager, "SoftwareOnlyRender", true);
        }
    }

    private static bool IsWindows7(Version osVersion)
    {
        if (osVersion.Major == 6)
        {
            return osVersion.Minor == 1;
        }
        return false;
    }

    public void Save(DatabaseConnectionManager connectionManager, string name, object value)
    {
        using IDbConnection cnn = connectionManager.OpenConnection();
        var param = new
        {
            name = name,
            value = value?.ToString()
        };
        if (cnn.Execute("UPDATE Settings SET Value=@Value WHERE Name=@Name", param) == 0)
        {
            cnn.Execute("INSERT INTO Settings (Name, Value) VALUES (@Name, @Value)", param);
        }
    }
}
