using System;
using System.IO;
using System.Threading;
using Mono.Unix.Native;

namespace SwissPension.IpcPrototype.Library
{
    public class IpcServer : IDisposable
    {
        private readonly string _requestPipe;
        private readonly string _responsePipe;

        public IpcServer(string pipeName)
        {
            var tempDir = Path.GetTempPath();
            _requestPipe = Path.Combine(tempDir, $"{pipeName}_in");
            _responsePipe = Path.Combine(tempDir, $"{pipeName}_out");

            // Ensure old pipes are removed
            Cleanup();

            // Create new pipes
            Syscall.mkfifo(_requestPipe, FilePermissions.S_IRWXU);
            Syscall.mkfifo(_responsePipe, FilePermissions.S_IRWXU);
        }

        public void RunLoop(CancellationToken token)
        {
            Console.WriteLine("Waiting for IPC client...");

            while (!token.IsCancellationRequested)
            {
                using var readStream = new FileStream(_requestPipe, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(readStream);
                var request = reader.ReadLine();

                if (request == null || request == "exit")
                    break;

                using var writeStream = new FileStream(_responsePipe, FileMode.Open, FileAccess.Write);
                using var writer = new StreamWriter(writeStream) { AutoFlush = true };

                writer.WriteLine(request == "hello" ? "Hello from DemoLibrary" : "Unknown command");
            }
        }

        public void Dispose()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (File.Exists(_requestPipe)) File.Delete(_requestPipe);
            if (File.Exists(_responsePipe)) File.Delete(_responsePipe);
        }
    }
}