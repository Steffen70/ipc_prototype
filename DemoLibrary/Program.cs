using System;
using System.Threading;

namespace SwissPension.IpcPrototype.Library
{
    static class Program
    {
        static void Main()
        {
            using var cts = new CancellationTokenSource();
            using var server = new IpcServer("demo_ipc");

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("\nExiting...");
                cts.Cancel();
                cts.Dispose();
                server.Dispose();
            };

            server.RunLoop(cts.Token);
        }
    }
}