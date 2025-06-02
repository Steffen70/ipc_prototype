using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SwissPension.IpcInterfaceBridge;

public class IpcInterfaceHost<TInterface> : IpcInterfaceBridge<TInterface>, IDisposable
{
    private readonly TInterface _instance;

    public IpcInterfaceHost(IIpcTransport transport, TInterface instance) : base(transport)
    {
        _instance = instance;
        Transport.EnsureCreated(RequestPipe);
        Transport.EnsureCreated(ResponsePipe);
        
        Initialize();
    }

    public void Dispose()
    {
        Transport.Cleanup(RequestPipe);
        Transport.Cleanup(ResponsePipe);
    }

    protected override void AddAction(string key, string methodName, Type[] parameterTypes, Type returnType)
    {
        Console.WriteLine($"Adding action: {key} - {methodName}");
        
        var method = typeof(TInterface).GetMethod(methodName, parameterTypes);
        AvailableFunctions[key] = args => method!.Invoke(_instance, args);
    }

    public Task<TReturnType> InvokeMethodAsync<TReturnType>(string hash, object[] parameters)
    {
        var result = ExecuteFunction(hash, parameters);
        return Task.FromResult((TReturnType)result);
    }

    public async void RunLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var json = await Transport.ReadAsync(RequestPipe, token);
            var call = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            var hash = call["Hash"].ToString();
            var jArray = (JArray)call["Params"];
            var parameters = jArray.ToObject<object[]>();

            var result = ExecuteFunction(hash, parameters);
            var resultJson = JsonConvert.SerializeObject(result);

            await Transport.WriteAsync(ResponsePipe, resultJson, token);
        }
    }
}