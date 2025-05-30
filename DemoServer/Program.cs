using System;
using System.Diagnostics;
using System.IO;

#if UNIX_IPC
using System.Threading.Tasks;

#else
using System.IO.Pipes;
#endif

const string pipeName = "demo_ipc";

#if UNIX_IPC
var temp = Path.GetTempPath();
var requestPipe = Path.Combine(temp, $"{pipeName}_in");
var responsePipe = Path.Combine(temp, $"{pipeName}_out");

if (!File.Exists(requestPipe))
{
    Console.WriteLine("Starting IPC server via Mono...");

    var serverProcess = new Process
    {
        StartInfo = new()
        {
            FileName = "mono",
            Arguments = $"{AppContext.BaseDirectory}/SwissPension.IpcPrototype.Library.exe",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    };

    serverProcess.Start();

    // Wait for requestPipe to appear
    var timeout = TimeSpan.FromSeconds(5);
    var sw = Stopwatch.StartNew();
    while (!File.Exists(requestPipe))
    {
        if (sw.Elapsed > timeout)
        {
            Console.Error.WriteLine("Timeout: IPC server pipe was not created.");
            return;
        }

        await Task.Delay(100);
    }
}

await using (var requestStream = new StreamWriter(requestPipe) { AutoFlush = true })
{
    await requestStream.WriteLineAsync("hello");
}

using (var responseStream = new StreamReader(responsePipe))
{
    var response = await responseStream.ReadLineAsync();
    Console.WriteLine($"Received: {response}");
}

await using (var exitStream = new StreamWriter(requestPipe) { AutoFlush = true })
{
    await exitStream.WriteLineAsync("exit");
}
#else
var requestPipeName = $"{pipeName}_in";
var responsePipeName = $"{pipeName}_out";

Console.WriteLine("Starting IPC server...");

var serverProcess = new Process
{
    StartInfo = new()
    {
        // no Mono on Windows
        FileName = "SwissPension.IpcPrototype.Library.exe", 
        WorkingDirectory = AppContext.BaseDirectory,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    }
};

serverProcess.Start();

using (var requestPipe = new NamedPipeClientStream(".", requestPipeName, PipeDirection.Out))
{
    await requestPipe.ConnectAsync(5000);
    await using var writer = new StreamWriter(requestPipe) { AutoFlush = true };
    await writer.WriteLineAsync("hello");
}

using (var responsePipe = new NamedPipeClientStream(".", responsePipeName, PipeDirection.In))
{
    await responsePipe.ConnectAsync(5000);
    using var reader = new StreamReader(responsePipe);
    var response = await reader.ReadLineAsync();
    Console.WriteLine($"Received: {response}");
}

using (var exitPipe = new NamedPipeClientStream(".", requestPipeName, PipeDirection.Out))
{
    await exitPipe.ConnectAsync(5000);
    await using var writer = new StreamWriter(exitPipe) { AutoFlush = true };
    await writer.WriteLineAsync("exit");
}
#endif