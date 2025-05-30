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
dotnet run
# Received: Hello from DemoLibrary
```

### .NET Framework (Mono) Project

**Note:** You don't need to run the Mono service seperatly, the `DemoServer` will automatically start `DemoLibrary` when it starts.

```bash
cd DemoLibrary
msbuild > /dev/null 2>&1 && mono ./bin/Debug/SwissPension.IpcPrototype.Library.exe
# Waiting for IPC client...
```

## Notes

-   The Mono project uses old-style `.csproj` and targets `net48`.
-   The .NET 8 project uses the modern SDK-style format.
-   Communication between processes can be implemented using named pipes on Windows and Unix domain sockets on Linux.

## Running on Windows

If you're using Windows, you don't need Mono—just install .NET 8 and .NET Framework 4.8 using the Visual Studio 2022 Installer. Then open IpcPrototype.sln, set DemoServer as the startup project, and run it. The build and IPC communication will work out of the box, thanks to a PowerShell 7 script that ensures cross-platform compatibility without relying on Bash or legacy Windows PowerShell.
