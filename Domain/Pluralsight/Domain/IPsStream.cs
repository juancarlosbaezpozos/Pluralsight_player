using System.IO;

namespace Pluralsight.Domain;

public interface IPsStream
{
	long Length { get; }

	int BlockSize { get; }

	void Seek(int offset, SeekOrigin begin);

	int Read(byte[] pv, int i, int count);

	void Dispose();
}
