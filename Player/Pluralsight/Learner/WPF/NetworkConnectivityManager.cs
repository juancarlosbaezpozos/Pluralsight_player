using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using NETWORKLIST;

namespace Pluralsight.Learner.WPF;

internal class NetworkConnectivityManager : INetworkListManagerEvents, IDisposable
{
	private readonly INetworkListManager networkListManager;

	private readonly IConnectionPoint connectionPoint;

	private int cookie;

	private bool? lastCheckWasConnectedToInternet;

	public event Action<bool> OnConnectivityChanged;

	public NetworkConnectivityManager()
	{
		networkListManager = (NetworkListManager)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("DCB00C01-570F-4A9B-8D69-199FDBA5723B")));
		IConnectionPointContainer obj = (IConnectionPointContainer)networkListManager;
		Guid riid = typeof(INetworkListManagerEvents).GUID;
		obj.FindConnectionPoint(ref riid, out connectionPoint);
		connectionPoint.Advise(this, out cookie);
	}

	public void ForceCheckNow()
	{
		bool isConnectedToInternet = networkListManager.IsConnectedToInternet;
		if (isConnectedToInternet != lastCheckWasConnectedToInternet)
		{
			lastCheckWasConnectedToInternet = isConnectedToInternet;
			this.OnConnectivityChanged?.Invoke(isConnectedToInternet);
		}
	}

	public void ConnectivityChanged(NLM_CONNECTIVITY newConnectivity)
	{
		ForceCheckNow();
	}

	public void Dispose()
	{
		if (cookie == 0)
		{
			return;
		}
		try
		{
			connectionPoint.Unadvise(cookie);
			cookie = 0;
		}
		catch
		{
		}
	}
}
