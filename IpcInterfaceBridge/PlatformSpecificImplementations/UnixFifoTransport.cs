using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SwissPension.IpcInterfaceBridge.PlatformSpecificImplementations;

public class UnixFifoTransport : IIpcTransport
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> PipeLocks = new();

    /// <summary>
    ///     Ensures that the FIFO pipe is created on Unix-based systems.
    ///     This method must be implemented in a .NET Framework–targeted project with a reference to Mono.Posix,
    ///     as .NET Standard does not support native syscall bindings like mkfifo.
    /// </summary>
    public virtual void EnsureCreated(string pipePath) => throw new NotImplementedException("EnsureCreated is implemented in the Unix specific SwissPension.IpcInterfaceBridge.PlatformSpecificImplementations.Unix.dll assembly.");

    public void Cleanup(string pipePath)
    {
        if (File.Exists(pipePath)) File.Delete(pipePath);
    }

    public async Task<string> ReadAsync(string pipePath, CancellationToken token = default)
    {
        var semaphore = PipeLocks.GetOrAdd(pipePath, _ => new(1, 1));
        await semaphore.WaitAsync(token);
        try
        {
            using var stream = new FileStream(pipePath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error reading from pipe: {e.Message}");
            throw;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task WriteAsync(string pipePath, string response, CancellationToken token = default)
    {
        var semaphore = PipeLocks.GetOrAdd(pipePath, _ => new(1, 1));
        await semaphore.WaitAsync(token);
        try
        {
            using var stream = new FileStream(pipePath, FileMode.Open, FileAccess.Write);
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            await writer.WriteAsync(response);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error writing to pipe: {e.Message}");
            throw;
        }
        finally
        {
            semaphore.Release();
        }
    }
}