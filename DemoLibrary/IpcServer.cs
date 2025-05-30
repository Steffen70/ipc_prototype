using System;
using System.IO;
using System.Threading;

#if UNIX_IPC
using Mono.Unix.Native;

#else
using System.IO.Pipes;
#endif

namespace SwissPension.IpcPrototype.Library
{
    public class IpcServer : IDisposable
    {
        private readonly string _requestPipe;
        private readonly string _responsePipe;

        public IpcServer(string pipeName)
        {
#if UNIX_IPC
            var tempDir = Path.GetTempPath();
            _requestPipe = Path.Combine(tempDir, $"{pipeName}_in");
            _responsePipe = Path.Combine(tempDir, $"{pipeName}_out");

            // Ensure old pipes are removed
            Cleanup();

            // Create new pipes
            Syscall.mkfifo(_requestPipe, FilePermissions.S_IRWXU);
            Syscall.mkfifo(_responsePipe, FilePermissions.S_IRWXU);
#else
            _requestPipe = pipeName + "_in";
            _responsePipe = pipeName + "_out";
#endif
        }

        public void RunLoop(CancellationToken token)
        {
            Console.WriteLine("Waiting for IPC client...");

            while (!token.IsCancellationRequested)
            {
#if UNIX_IPC
                using var readStream = new FileStream(_requestPipe, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(readStream);
                var request = reader.ReadLine();
#else
                using var requestPipe = new NamedPipeServerStream(_requestPipe, PipeDirection.In);
                requestPipe.WaitForConnection();

                using var reader = new StreamReader(requestPipe);
                var request = reader.ReadLine();
#endif

                if (request == null || request == "exit")
                    break;

#if UNIX_IPC
                using var writeStream = new FileStream(_responsePipe, FileMode.Open, FileAccess.Write);
#else
                using var writeStream = new NamedPipeServerStream(_responsePipe, PipeDirection.Out);
                writeStream.WaitForConnection();
#endif

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
#if UNIX_IPC
            if (File.Exists(_requestPipe)) File.Delete(_requestPipe);
            if (File.Exists(_responsePipe)) File.Delete(_responsePipe);
#endif
        }
    }
}