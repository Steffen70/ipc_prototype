using System.IO;
using Mono.Unix.Native;

#if !UNIX_IPC
using System;
#endif

namespace SwissPension.IpcInterfaceBridge.PlatformSpecificImplementations.Unix
{
    public class UnixFifoTransport : SwissPension.IpcInterfaceBridge.PlatformSpecificImplementations.UnixFifoTransport
    {
        public override void EnsureCreated(string pipePath)
        {
#if UNIX_IPC
            if (File.Exists(pipePath)) File.Delete(pipePath);
            Syscall.mkfifo(pipePath, FilePermissions.S_IRWXU);
#else
            throw new PlatformNotSupportedException("UnixFifiTransport is intended to run on Unix systems. Build this project on a Unix system with Mono.");
#endif
        }
    }
}