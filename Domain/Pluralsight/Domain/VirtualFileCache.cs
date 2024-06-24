using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Pluralsight.Domain;

public class VirtualFileCache : IDisposable
{
    private readonly IPsStream encryptedVideoFile;

    public long Length => encryptedVideoFile.Length;

    public VirtualFileCache(string encryptedVideoFilePath)
    {
        encryptedVideoFile = new PsStream(encryptedVideoFilePath);
    }

    public VirtualFileCache(IPsStream stream)
    {
        encryptedVideoFile = stream;
    }

    public void Read(byte[] pv, int offset, int count, IntPtr pcbRead)
    {
        if (Length != 0L)
        {
            encryptedVideoFile.Seek(offset, SeekOrigin.Begin);
            int num = encryptedVideoFile.Read(pv, 0, count);

            if (IntPtr.Zero != pcbRead)
            {
                Marshal.WriteIntPtr(pcbRead, new IntPtr(num));
            }
        }
    }

    public void Dispose()
    {
        encryptedVideoFile.Dispose();
    }
}
