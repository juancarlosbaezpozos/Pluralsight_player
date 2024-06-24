using System;

namespace Pluralsight.Domain.WPF.Persistance;

public static class DiskLocations
{
    public static string SettingsLocation()
    {
        return AppDataDirectory() + "\\settings\\";
    }

    public static string DefaultDownloadLocationRoot()
    {
        return AppDataDirectory() + "\\";
    }

    public static string DatabaseLocation()
    {
        return AppDataDirectory() + "\\pluralsight.db";
    }

    private static string AppDataDirectory()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Pluralsight";
    }
}
