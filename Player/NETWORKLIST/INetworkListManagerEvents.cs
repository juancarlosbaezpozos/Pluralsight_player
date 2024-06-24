using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NETWORKLIST;

[ComImport]
[CompilerGenerated]
[Guid("DCB00001-570F-4A9B-8D69-199FDBA5723B")]
[InterfaceType(1)]
[TypeIdentifier]
public interface INetworkListManagerEvents
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void ConnectivityChanged([In] NLM_CONNECTIVITY newConnectivity);
}
