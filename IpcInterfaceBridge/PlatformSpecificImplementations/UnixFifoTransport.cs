using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SwissPension.IpcInterfaceBridge.PlatformSpecificImplementations
{
    public class UnixFifoTransport : IpcTransport
    {
        public override (string requestPipe, string responsePipe) GetPipePaths(string pipeName)
        {
            var temp = Path.GetTempPath();
            var requestPipe = Path.Combine(temp, $"{pipeName}_in");
            var responsePipe = Path.Combine(temp, $"{pipeName}_out");

            return (requestPipe, responsePipe);
        }

        public override void EnsureCreated(string pipePath) => throw new NotImplementedException("UnixFifoTransport.EnsureCreated is implemented in a separate platform-specific assembly.");

        public override void Cleanup(string pipePath)
        {
            if (File.Exists(pipePath)) File.Delete(pipePath);
        }

        protected override async Task<string> _ReadAsync(string pipePath, CancellationToken token = default)
        {
            using (var stream = new FileStream(pipePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream))
                {
                    var readTask = reader.ReadLineAsync();
                    var completedTask = await Task.WhenAny(readTask, Task.Delay(Timeout.Infinite, token));

                    if (completedTask == readTask)
                        // Already completed
                        return await readTask;

                    // Timeout or cancellation
                    token.ThrowIfCancellationRequested();

                    // This line is technically unreachable, but needed to satisfy the compiler
                    return null;
                }
            }
        }

        protected override async Task _WriteAsync(string pipePath, string response, CancellationToken token = default)
        {
            using (var stream = new FileStream(pipePath, FileMode.Open, FileAccess.Write))
            {
                using (var writer = new StreamWriter(stream) { AutoFlush = true })
                {
                    await writer.WriteLineAsync(response);
                }
            }
        }
    }
}