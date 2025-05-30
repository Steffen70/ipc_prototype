using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

var temp = Path.GetTempPath();
var requestPipe = Path.Combine(temp, "demo_ipc_in");
var responsePipe = Path.Combine(temp, "demo_ipc_out");

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