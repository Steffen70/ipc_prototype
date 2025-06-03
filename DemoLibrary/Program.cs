using System;
using System.Threading;
using SwissPension.IpcInterfaceBridge;
using SwissPension.IpcInterfaceBridge.PlatformSpecificImplementations;
using SwissPension.IpcPrototype.Common;

namespace SwissPension.IpcPrototype.Library
{
    internal static class Program
    {
        private static void Main()
        {
            using (var cts = new CancellationTokenSource())
            {
                var demoLibrary = new DemoLibrary();

                using (var ipcHost = new IpcInterfaceHost<IDemoLibrary>(demoLibrary))
                {
                    if (ipcHost.Transport is UnixFifoTransport)
                        ipcHost.Transport = new IpcInterfaceBridge.PlatformSpecificImplementations.Unix.UnixFifoTransport();
                    
                    Console.CancelKeyPress += (_, e) =>
                    {
                        e.Cancel = true;
                        Console.WriteLine("Exiting...");
                        cts.Cancel();
                        ipcHost.Dispose();
                        cts.Dispose();
                    };

                    ipcHost.RunLoopAsync(cts.Token).GetAwaiter().GetResult();
                }
            }
        }
    }
}