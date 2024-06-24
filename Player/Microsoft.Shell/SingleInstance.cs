using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Microsoft.Shell;

public static class SingleInstance<TApplication> where TApplication : Application, ISingleInstanceApp
{
    private class IPCRemoteService : MarshalByRefObject
    {
        public void InvokeFirstInstance(IList<string> args)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(SingleInstance<TApplication>.ActivateFirstInstanceCallback), args);
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }

    private const string Delimiter = ":";

    private const string ChannelNameSuffix = "SingeInstanceIPCChannel";

    private const string RemoteServiceName = "SingleInstanceApplicationService";

    private const string IpcProtocol = "ipc://";

    private static Mutex singleInstanceMutex;

    private static IpcServerChannel channel;

    private static IList<string> commandLineArgs;

    public static IList<string> CommandLineArgs => commandLineArgs;

    public static bool InitializeAsFirstInstance(string uniqueName)
    {
        commandLineArgs = GetCommandLineArgs(uniqueName);
        string text = uniqueName + Environment.UserName;
        string channelName = text + ":" + "SingeInstanceIPCChannel";
        singleInstanceMutex = new Mutex(initiallyOwned: true, text, out var createdNew);
        if (createdNew)
        {
            CreateRemoteService(channelName);
        }
        else
        {
            SignalFirstInstance(channelName, commandLineArgs);
        }
        return createdNew;
    }

    public static void Cleanup()
    {
        if (singleInstanceMutex != null)
        {
            singleInstanceMutex.Close();
            singleInstanceMutex = null;
        }
        if (channel != null)
        {
            ChannelServices.UnregisterChannel(channel);
            channel = null;
        }
    }

    private static IList<string> GetCommandLineArgs(string uniqueApplicationName)
    {
        string[] array = null;
        if (AppDomain.CurrentDomain.ActivationContext == null)
        {
            array = Environment.GetCommandLineArgs();
        }
        else
        {
            string path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), uniqueApplicationName), "cmdline.txt");
            if (File.Exists(path))
            {
                try
                {
                    using (TextReader textReader = new StreamReader(path, Encoding.Unicode))
                    {
                        array = NativeMethods.CommandLineToArgvW(textReader.ReadToEnd());
                    }
                    File.Delete(path);
                }
                catch (IOException)
                {
                }
            }
        }
        if (array == null)
        {
            array = new string[0];
        }
        return new List<string>(array);
    }

    private static void CreateRemoteService(string channelName)
    {
        BinaryServerFormatterSinkProvider binaryServerFormatterSinkProvider = new BinaryServerFormatterSinkProvider();
        binaryServerFormatterSinkProvider.TypeFilterLevel = TypeFilterLevel.Full;
        Dictionary<string, string> strs = new Dictionary<string, string>();
        ((IDictionary)strs)["name"] = channelName;
        ((IDictionary)strs)["portName"] = channelName;
        ((IDictionary)strs)["exclusiveAddressUse"] = "false";
        SingleInstance<TApplication>.channel = new IpcServerChannel(strs, binaryServerFormatterSinkProvider);
        ChannelServices.RegisterChannel(channel, ensureSecurity: true);
        RemotingServices.Marshal(new IPCRemoteService(), "SingleInstanceApplicationService");
    }

    private static void SignalFirstInstance(string channelName, IList<string> args)
    {
        ChannelServices.RegisterChannel(new IpcClientChannel(), ensureSecurity: true);
        string url = "ipc://" + channelName + "/SingleInstanceApplicationService";
        ((IPCRemoteService)RemotingServices.Connect(typeof(IPCRemoteService), url))?.InvokeFirstInstance(args);
    }

    private static object ActivateFirstInstanceCallback(object arg)
    {
        ActivateFirstInstance(arg as IList<string>);
        return null;
    }

    private static void ActivateFirstInstance(IList<string> args)
    {
        if (Application.Current != null)
        {
            ((TApplication)Application.Current).SignalExternalCommandLineArgs(args);
        }
    }
}
