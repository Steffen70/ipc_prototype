using System;
using System.Threading;
using SwissPension.IpcInterfaceBridge;
using SwissPension.IpcInterfaceBridge.PlatformSpecificImplementations.Unix;
using SwissPension.IpcPrototype.Common;

namespace SwissPension.IpcPrototype.Library
{
    static class Program
    {
        static void Main()
        {
            using var cts = new CancellationTokenSource();
            var transport = new UnixFifoTransport();
            var demoLibrary = new DemoLibrary();
            using var ipcHost = new IpcInterfaceHost<IDemoLibrary>(transport, demoLibrary);

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("\nExiting...");
                cts.Cancel();
                cts.Dispose();
                ipcHost.Dispose();
            };

            ipcHost.RunLoopAsync(cts.Token);
        }
    }
}