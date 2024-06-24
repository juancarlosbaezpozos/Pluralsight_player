using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Shell;

[SuppressUnmanagedCodeSecurity]
internal static class NativeMethods
{
	public delegate IntPtr MessageHandler(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled);

	[DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "CommandLineToArgvW")]
	private static extern IntPtr _CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string cmdLine, out int numArgs);

	[DllImport("kernel32.dll", EntryPoint = "LocalFree", SetLastError = true)]
	private static extern IntPtr _LocalFree(IntPtr hMem);

	public static string[] CommandLineToArgvW(string cmdLine)
	{
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			int numArgs = 0;
			intPtr = _CommandLineToArgvW(cmdLine, out numArgs);
			if (intPtr == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			string[] array = new string[numArgs];
			for (int i = 0; i < numArgs; i++)
			{
				IntPtr ptr = Marshal.ReadIntPtr(intPtr, i * Marshal.SizeOf(typeof(IntPtr)));
				array[i] = Marshal.PtrToStringUni(ptr);
			}
			return array;
		}
		finally
		{
			_LocalFree(intPtr);
		}
	}
}
