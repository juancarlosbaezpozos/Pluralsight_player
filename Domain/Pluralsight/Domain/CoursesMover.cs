using System;
using System.IO;
using System.Linq;

namespace Pluralsight.Domain;

public class CoursesMover
{
    private long totalFilesToMove;

    private long totalFilesMoved;

    public event Action<double> ProgressPercent;

    public event Action Complete;

    public void MoveAll(DirectoryInfo source, DirectoryInfo destination)
    {
        try
        {
            totalFilesToMove = GetFilesInDirectory(source);
            totalFilesMoved = 0L;
            MoveAllFilesForFolder(source, destination);
        }
        finally
        {
            this.Complete?.Invoke();
        }
    }

    private void MoveAllFilesForFolder(DirectoryInfo source, DirectoryInfo destination)
    {
        if (!destination.Exists)
        {
            destination.Create();
        }
        if (!source.Exists)
        {
            return;
        }
        FileInfo[] files = source.GetFiles();
        foreach (FileInfo fileInfo in files)
        {
            try
            {
                fileInfo.CopyTo(Path.Combine(destination.ToString(), fileInfo.Name), overwrite: true);
                fileInfo.Delete();
            }
            catch (Exception)
            {
            }
            totalFilesMoved++;
            this.ProgressPercent?.Invoke((double)totalFilesMoved / (double)totalFilesToMove);
        }
        DirectoryInfo[] directories = source.GetDirectories();
        foreach (DirectoryInfo directoryInfo in directories)
        {
            DirectoryInfo destination2 = destination.CreateSubdirectory(directoryInfo.Name);
            MoveAllFilesForFolder(directoryInfo, destination2);
            try
            {
                directoryInfo.Delete();
            }
            catch (Exception)
            {
            }
        }
        source.Delete();
    }

    private static long GetSizeOfDirectory(DirectoryInfo source)
    {
        long num = 0L;
        FileInfo[] files = source.GetFiles();
        foreach (FileInfo fileInfo in files)
        {
            num += fileInfo.Length;
        }
        DirectoryInfo[] directories = source.GetDirectories();
        foreach (DirectoryInfo source2 in directories)
        {
            num += GetSizeOfDirectory(source2);
        }
        return num;
    }

    private static long GetFilesInDirectory(DirectoryInfo source)
    {
        long num;
        try
        {
            num = source.GetFiles().LongCount();
        }
        catch (Exception)
        {
            return 0L;
        }
        DirectoryInfo[] directories = source.GetDirectories();
        foreach (DirectoryInfo source2 in directories)
        {
            num += GetFilesInDirectory(source2);
        }
        return num;
    }
}
