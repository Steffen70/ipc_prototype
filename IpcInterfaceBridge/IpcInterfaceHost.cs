using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SwissPension.IpcInterfaceBridge
{
    public class IpcInterfaceHost<TInterface> : IpcInterfaceBridge<TInterface>, IDisposable
    {
        private readonly TInterface _instance;

        public IpcInterfaceHost(TInterface instance)
        {
            _instance = instance;
            Initialize();
        }

        public void Dispose()
        {
            var (requestPipe, responsePipe) = Transport.GetPipePaths(PipeName);
            Transport.Cleanup(requestPipe);
            Transport.Cleanup(responsePipe);
        }

        protected override void AddAction(string key, string methodName, Type[] parameterTypes, Type returnType)
        {
            Console.WriteLine($"Adding action: {key} - {methodName}");

            var method = typeof(TInterface).GetMethod(methodName, parameterTypes);

            Debug.Assert(method != null);

            AvailableFunctions[key] = args => method.Invoke(_instance, args);
        }

        public async Task RunLoopAsync(CancellationToken token)
        {
            var (requestPipe, responsePipe) = Transport.GetPipePaths(PipeName);
            Transport.EnsureCreated(requestPipe);
            Transport.EnsureCreated(responsePipe);

            while (!token.IsCancellationRequested)
            {
                var json = await Transport.ReadAsync(requestPipe, token);

                if (token.IsCancellationRequested)
                    break;

                var call = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                var hash = call["Hash"].ToString();
                var jArray = (JArray)call["Params"];
                var parameters = jArray.ToObject<object[]>();

                var result = await ExecuteFunctionAsync(hash, parameters);
                var resultJson = result == null ? string.Empty : JsonConvert.SerializeObject(result);

                await Transport.WriteAsync(responsePipe, resultJson, token);
            }
        }
    }
}