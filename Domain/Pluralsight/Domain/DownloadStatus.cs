namespace Pluralsight.Domain;

public enum DownloadStatus
{
    Unknown,
    UnknownError,
    NetworkError,
    DiskError,
    Success
}
