using System;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Dapper;
using Pluralsight.Domain.WPF.Persistance.Migrations;

namespace Pluralsight.Domain.WPF.Persistance;

public class DatabaseConnectionManager
{
    private static readonly IMigration[] Migrations = new IMigration[15]
    {
        new InitialState(),
        new CreateUserProfileTable(),
        new CreateCourseTable(),
        new CreateModuleAndClipTables(),
        new CreateCourseProgressTable(),
        new CreateClipViewTable(),
        new AlterCourseTableAddImageUrls(),
        new CreateCourseAccessTable(),
        new CreateClipTranscriptTable(),
        new CreateSettingsTable(),
        new AlterUserAddUserHandle(),
        new SetHardwareAccelerationOffForWindows7(),
        new AlterCourseAddCache(),
        new CreateAnalyticsTable(),
        new AlterCourseAddDownload()
    };

    public static string DatabaseFile
    {
        get
        {
            string text = ConfigurationManager.AppSettings["DatabaseLocation"];
            if (string.IsNullOrEmpty(text) || !Path.IsPathRooted(text))
            {
                return DiskLocations.DatabaseLocation();
            }
            try
            {
                return Path.GetFullPath(text);
            }
            catch (Exception)
            {
                return DiskLocations.DatabaseLocation();
            }
        }
    }

    public DatabaseConnectionManager()
    {
        if (!File.Exists(DatabaseFile))
        {
            new FileInfo(DatabaseFile).Directory?.Create();
            SQLiteConnection.CreateFile(DatabaseFile);
        }
    }

    public void DeleteDatabase()
    {
        try
        {
            new FileInfo(DatabaseFile).Delete();
        }
        catch
        {
        }
    }

    public void UpdateDatabase()
    {
        long currentVersion = GetCurrentDatabaseVersion();
        foreach (IMigration item in from x in Migrations
                                    orderby x.VersionNumber
                                    where x.VersionNumber > currentVersion
                                    select x)
        {
            item.ApplyMigration(this);
            UpdateDatabaseVersion(item.VersionNumber);
        }
    }

    private void UpdateDatabaseVersion(int migrationVersionNumber)
    {
        using IDbConnection cnn = OpenConnection();
        cnn.Execute("INSERT INTO Version (CurrentVersion) VALUES (@version)", new
        {
            version = migrationVersionNumber
        });
    }

    private long GetCurrentDatabaseVersion()
    {
        try
        {
            using IDbConnection dbConnection = OpenConnection();
            using IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = "select max(CurrentVersion) from Version;";
            return (long)dbCommand.ExecuteScalar();
        }
        catch
        {
            return 0L;
        }
    }

    public IDbConnection OpenConnection()
    {
        SQLiteConnection sQLiteConnection = new SQLiteConnection("Data Source=" + DatabaseFile + ";");
        sQLiteConnection.Open();
        return sQLiteConnection;
    }

    public DbConnectionTs Open()
    {
        SQLiteConnection sQLiteConnection = new SQLiteConnection("Data Source=" + DatabaseFile + ";");
        sQLiteConnection.Open();
        return new DbConnectionTs
        {
            Connection = sQLiteConnection
        };
    }
}
