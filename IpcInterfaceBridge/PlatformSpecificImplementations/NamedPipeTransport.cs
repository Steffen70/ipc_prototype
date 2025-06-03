using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace SwissPension.IpcInterfaceBridge.PlatformSpecificImplementations
{
    public class NamedPipeTransport : IpcTransport
    {
        private readonly bool _isIpcServer;

        public NamedPipeTransport(bool isIpcServer)
        {
            _isIpcServer = isIpcServer;
        }

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
            using (var pipe = _isIpcServer ? (Stream)new NamedPipeServerStream(pipeName, PipeDirection.In) : new NamedPipeClientStream(".", pipeName, PipeDirection.In))
            {
                switch (pipe)
                {
                    case NamedPipeServerStream serverStream:
                        await serverStream.WaitForConnectionAsync(token);
                        break;
                    case NamedPipeClientStream clientStream:
                        await clientStream.ConnectAsync(token);
                        break;
                    default: throw new InvalidOperationException("Invalid pipe type");
                }

                using (var reader = new StreamReader(pipe))
                {
                    return await reader.ReadLineAsync();
                }
            }
        }

        protected override async Task _WriteAsync(string pipeName, string response, CancellationToken token = default)
        {
            using (var pipe = _isIpcServer ? (Stream)new NamedPipeServerStream(pipeName, PipeDirection.Out) : new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                switch (pipe)
                {
                    case NamedPipeServerStream serverStream:
                        await serverStream.WaitForConnectionAsync(token);
                        break;
                    case NamedPipeClientStream clientStream:
                        await clientStream.ConnectAsync(token);
                        break;
                    default: throw new InvalidOperationException("Invalid pipe type");
                }

                using (var writer = new StreamWriter(pipe) { AutoFlush = true })
                {
                    await writer.WriteLineAsync(response);
                }
            }
        }
    }
}