# IPC Prototype (C# / .NET 8 + Mono)

This is a simple cross-platform prototype demonstrating inter-process communication between a .NET 8 console application and a .NET Framework 4.8 executable running on Mono.

## Requirements

This setup guide assumes you're using **Kubuntu 24.04** or another Debian-based Linux distribution.

### .NET 8 SDK

Install the SDK manually:

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0 --install-dir ~/.dotnet
```

Add to your shell config:

```bash
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"
```

### Mono 6.12+

Follow the official instructions to add the Mono APT repository:
[https://www.mono-project.com/download/stable/#download-lin-debian](https://www.mono-project.com/download/stable/#download-lin-debian)

Then install Mono:

```bash
sudo apt update
sudo apt install mono-devel
```

## Running the Projects

### .NET 8 Project

```bash
cd DemoServer
dotnet run --framework net8.0
```

### .NET Framework (Mono) Project

**Note:** You don't need to run the Mono service seperatly, the `DemoServer` will automatically start `DemoLibrary` when it starts.

```bash
cd DemoLibrary
msbuild /t:rebuild > /dev/null 2>&1 && mono ./bin/Debug/SwissPension.IpcPrototype.Library.exe
```

## Running on Windows

If you're using Windows, you don't need Mono—just install .NET 8 and .NET Framework 4.8 using the Visual Studio 2022 Installer. Then open IpcPrototype.sln, set DemoLibrary and DemoServer as the startup projects, and run them simultaneously.

## TODO

### Pipe Lock Bottlenecks

The current implementation uses a per-pipe `SemaphoreSlim` to serialize read and write operations, ensuring safe concurrent access. While this approach is simple and effective for low-volume or single-client scenarios, it risks becoming a performance bottleneck under high-throughput or multi-client conditions. In the future, this should be replaced with a more scalable solution—such as a pipe pool or connection queue—that allows multiple concurrent server-side readers and writers. Each handler could operate independently using a dedicated instance or ephemeral pipe name, enabling better parallelism and responsiveness in high-load environments.
