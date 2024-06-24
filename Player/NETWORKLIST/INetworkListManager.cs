using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NETWORKLIST;

[ComImport]
[CompilerGenerated]
[Guid("DCB00000-570F-4A9B-8D69-199FDBA5723B")]
[TypeIdentifier]
public interface INetworkListManager
{
	void _VtblGap1_4();

	[DispId(5)]
	bool IsConnectedToInternet
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(5)]
		get;
	}
}
