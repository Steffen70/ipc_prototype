using System;
#if UNIX_IPC
using Mono.Unix.Native;
#endif

namespace SwissPension.IpcInterfaceBridge.PlatformSpecificImplementations.Unix
{
    public class UnixFifoTransport : SwissPension.IpcInterfaceBridge.PlatformSpecificImplementations.UnixFifoTransport
    {
        public override void EnsureCreated(string pipePath)
        {
            if (!IsRunningOnMono)
                throw new PlatformNotSupportedException("UnixFifoTransport is only supported on Mono.");

            Cleanup(pipePath);

            try
            {
#if UNIX_IPC
                Syscall.mkfifo(pipePath, FilePermissions.S_IRWXU);
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to call mkfifo via reflection: {ex}");
                throw;
            }
        }
    }
}