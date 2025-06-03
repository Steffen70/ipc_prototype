using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SwissPension.IpcInterfaceBridge
{
    public abstract class IpcTransport
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> PipeLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
        public static bool IsRunningOnMono => Type.GetType("Mono.Runtime") != null;

        public abstract (string requestPipe, string responsePipe) GetPipePaths(string pipeName);
        public abstract void EnsureCreated(string pipePath);
        public abstract void Cleanup(string pipePath);

        public async Task<string> ReadAsync(string pipePath, CancellationToken token = default)
        {
            var semaphore = PipeLocks.GetOrAdd(pipePath, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync(token);
            try
            {
                return await _ReadAsync(pipePath, token);
            }
            catch (Exception) when (token.IsCancellationRequested)
            {
                // Safe to ignore pipe errors during cancellation
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from pipe: {ex.Message}");
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }

        protected abstract Task<string> _ReadAsync(string pipePath, CancellationToken token = default);

        public async Task WriteAsync(string pipePath, string response, CancellationToken token = default)
        {
            var semaphore = PipeLocks.GetOrAdd(pipePath, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync(token);
            try
            {
                await _WriteAsync(pipePath, response, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to pipe: {ex.Message}");
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }

        protected abstract Task _WriteAsync(string pipePath, string response, CancellationToken token = default);
    }
}