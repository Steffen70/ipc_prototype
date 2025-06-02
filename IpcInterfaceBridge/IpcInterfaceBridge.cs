using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SwissPension.IpcInterfaceBridge;

public class IpcInterfaceBridge<TInterface>
{
    public readonly string RequestPipe;
    public readonly string ResponsePipe;

    protected readonly ConcurrentDictionary<string, Func<object[], object>> AvailableFunctions = new();

    protected readonly IIpcTransport Transport;

    public IpcInterfaceBridge(IIpcTransport transport)
    {
        Transport = transport;

        var interfaceName = typeof(TInterface).Name;
        var pipeName = $"{interfaceName}_pipe";

        var temp = Path.GetTempPath();
        RequestPipe = Path.Combine(temp, $"{pipeName}_in");
        ResponsePipe = Path.Combine(temp, $"{pipeName}_out");

        // Do not call Initialize() during base class construction if the current instance is a derived type.
        // This avoids calling potentially overridden virtual methods before the derived constructor has run.
        // The derived class is responsible for calling Initialize() manually after its own fields are initialized.
        if (GetType() == typeof(IpcInterfaceBridge<TInterface>))
            Initialize();
    }

    protected void Initialize()
    {
        foreach (var method in typeof(TInterface).GetMethods())
        {
            var methodName = method.Name;
            var parameters = method.GetParameters();
            var returnType = method.ReturnType;

            var hashInput = methodName + string.Join(",", parameters.Select(p => p.ParameterType.FullName)) + returnType.FullName;
            var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
            var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

            AddAction(hash, methodName, parameters.Select(p => p.ParameterType).ToArray(), returnType);
        }
    }

    protected virtual void AddAction(string key, string methodName, Type[] parameterTypes, Type returnType)
    {
        AvailableFunctions[key] = args =>
        {
            var taskType = typeof(Task<>).MakeGenericType(returnType);
            var method = typeof(IpcInterfaceBridge<TInterface>)
                .GetMethod(nameof(InvokeMethodOverIpcAsync), BindingFlags.Instance | BindingFlags.NonPublic)!
                .MakeGenericMethod(returnType);

            // Calls InvokeMethodOverIpcAsync<returnType>(key, args) and returns Task<object>
            return method.Invoke(this, [key, args]);
        };
    }

    protected async Task<TReturnType> InvokeMethodOverIpcAsync<TReturnType>(string hash, object[] parameters)
    {
        var payload = JsonConvert.SerializeObject(new { Hash = hash, Params = parameters });
        await Transport.WriteAsync(RequestPipe, payload);
        var resultJson = await Transport.ReadAsync(ResponsePipe);
        return JsonConvert.DeserializeObject<TReturnType>(resultJson);
    }

    public object ExecuteFunction(string hash, params object[] parameters)
    {
        if (AvailableFunctions.TryGetValue(hash, out var func))
            return func(parameters);

        throw new InvalidOperationException("Function not found for signature: " + hash);
    }
}