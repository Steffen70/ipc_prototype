using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace SwissPension.IpcInterfaceBridge.PlatformSpecificImplementations;

public class NamedPipeTransport : IIpcTransport
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> PipeLocks = new();

    public void EnsureCreated(string pipePath)
    {
        // nothing needed for named pipes
    }

    public void Cleanup(string pipePath)
    {
        // nothing needed for named pipes
    }

    public async Task<string> ReadAsync(string pipeName, CancellationToken token = default)
    {
        var semaphore = PipeLocks.GetOrAdd(pipeName, _ => new(1, 1));
        await semaphore.WaitAsync(token);
        try
        {
            using var pipe = new NamedPipeServerStream(pipeName, PipeDirection.In);
            await pipe.WaitForConnectionAsync(token);
            using var reader = new StreamReader(pipe);
            return await reader.ReadToEndAsync();
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task WriteAsync(string pipeName, string response, CancellationToken token = default)
    {
        var semaphore = PipeLocks.GetOrAdd(pipeName, _ => new(1, 1));
        await semaphore.WaitAsync(token);
        try
        {
            using var pipe = new NamedPipeServerStream(pipeName, PipeDirection.Out);
            await pipe.WaitForConnectionAsync(token);
            using var writer = new StreamWriter(pipe) { AutoFlush = true };
            await writer.WriteAsync(response);
        }
        finally
        {
            semaphore.Release();
        }
    }
}