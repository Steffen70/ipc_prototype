using System;
using SwissPension.IpcInterfaceBridge;
using SwissPension.IpcInterfaceBridge.PlatformSpecificImplementations;
using SwissPension.IpcPrototype.Common;

var transport = new UnixFifoTransport();
var ipcInterfaceBridge = new IpcInterfaceBridge<IDemoLibrary>(transport);

var result = ipcInterfaceBridge.ExecuteFunction("49330D0E257A19CB2A9F9EF2727C2C1A", "Steffen70");

var resultString = result as string;

Console.WriteLine($"Result: {resultString}");