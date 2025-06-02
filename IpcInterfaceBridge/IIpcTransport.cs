using System.Threading;
using System.Threading.Tasks;

namespace SwissPension.IpcInterfaceBridge;

public interface IIpcTransport
{
    Task<string> ReadAsync(string pipePath, CancellationToken token = default);
    Task WriteAsync(string pipePath, string response, CancellationToken token = default);
    void EnsureCreated(string pipePath);
    void Cleanup(string pipePath);
}