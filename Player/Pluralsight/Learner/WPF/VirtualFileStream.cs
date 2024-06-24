using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Pluralsight.Domain;

namespace Pluralsight.Learner.WPF;

internal class VirtualFileStream : IStream, IDisposable
{
	private long position;

	private readonly object _Lock = new object();

	private VirtualFileCache _Cache;

	public VirtualFileStream(string EncryptedVideoFilePath)
	{
		_Cache = new VirtualFileCache(EncryptedVideoFilePath);
	}

	private VirtualFileStream(VirtualFileCache Cache)
	{
		_Cache = Cache;
	}

	public void Read(byte[] pv, int cb, IntPtr pcbRead)
	{
		if (position < 0 || position > _Cache.Length)
		{
			Marshal.WriteIntPtr(pcbRead, new IntPtr(0));
			return;
		}
		lock (_Lock)
		{
			_Cache.Read(pv, (int)position, cb, pcbRead);
			position += pcbRead.ToInt64();
			Console.WriteLine();
		}
	}

	public void Write(byte[] pv, int cb, IntPtr pcbWritten)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Clone(out IStream ppstm)
	{
		ppstm = new VirtualFileStream(_Cache);
	}

	public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
	{
		lock (_Lock)
		{
			switch (dwOrigin)
			{
			case 0:
				position = dlibMove;
				break;
			case 1:
				position += dlibMove;
				break;
			case 2:
				position = _Cache.Length + dlibMove;
				break;
			}
			if (IntPtr.Zero != plibNewPosition)
			{
				Marshal.WriteInt64(plibNewPosition, position);
			}
		}
	}

	public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
	{
		pstatstg = default(System.Runtime.InteropServices.ComTypes.STATSTG);
		pstatstg.cbSize = _Cache.Length;
	}

	public void Commit(int grfCommitFlags)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void LockRegion(long libOffset, long cb, int dwLockType)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Revert()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetSize(long libNewSize)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void UnlockRegion(long libOffset, long cb, int dwLockType)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Dispose()
	{
		_Cache.Dispose();
	}
}
