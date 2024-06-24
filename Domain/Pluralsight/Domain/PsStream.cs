using System.IO;

namespace Pluralsight.Domain;

public class PsStream : IPsStream
{
	private readonly Stream fileStream;

	private long _length;

	public int BlockSize => 262144;

	public long Length => _length;

	public PsStream(string filenamePath)
	{
		fileStream = File.Open(filenamePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		_length = new FileInfo(filenamePath).Length;
	}

	public void Seek(int offset, SeekOrigin begin)
	{
		if (_length > 0)
		{
			fileStream.Seek(offset, begin);
		}
	}

	public int Read(byte[] pv, int i, int count)
	{
		if (_length <= 0)
		{
			return 0;
		}
		return fileStream.Read(pv, i, count);
	}

	public void Dispose()
	{
		_length = 0L;
		fileStream.Dispose();
	}
}
