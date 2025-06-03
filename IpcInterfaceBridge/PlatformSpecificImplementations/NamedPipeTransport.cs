using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace SwissPension.IpcInterfaceBridge.PlatformSpecificImplementations
{
    public class NamedPipeTransport : IpcTransport
    {
        public override (string requestPipe, string responsePipe) GetPipePaths(string pipeName) => ($"{pipeName}_in", $"{pipeName}_out");

        public override void EnsureCreated(string pipePath)
        {
            // nothing needed for named pipes
        }

        public override void Cleanup(string pipePath)
        {
            // nothing needed for named pipes
        }

        protected override async Task<string> _ReadAsync(string pipeName, CancellationToken token = default)
        {
            using (var pipe = new NamedPipeServerStream(pipeName, PipeDirection.In))
            {
                await pipe.WaitForConnectionAsync(token);
                using (var reader = new StreamReader(pipe))
                {
                    return await reader.ReadLineAsync();
                }
            }
        }

        protected override async Task _WriteAsync(string pipeName, string response, CancellationToken token = default)
        {
            using (var pipe = new NamedPipeServerStream(pipeName, PipeDirection.Out))
            {
                await pipe.WaitForConnectionAsync(token);
                using (var writer = new StreamWriter(pipe) { AutoFlush = true })
                {
                    await writer.WriteLineAsync(response);
                }
            }
        }
    }
}