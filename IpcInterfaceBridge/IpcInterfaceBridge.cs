using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SwissPension.IpcInterfaceBridge.PlatformSpecificImplementations;

namespace SwissPension.IpcInterfaceBridge
{
    public class IpcInterfaceBridge<TInterface>
    {
        public IpcTransport Transport { get; set; }

        protected readonly string PipeName = typeof(TInterface).Name;

        protected readonly ConcurrentDictionary<string, Func<object[], object>> AvailableFunctions = new ConcurrentDictionary<string, Func<object[], object>>();

        public IpcInterfaceBridge()
        {
#if UNIX_IPC
            Transport = new UnixFifoTransport();
#else
            Transport = new NamedPipeTransport();
#endif

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
                
                var hashInput = $"{methodName}({string.Join(", ", parameters.Select(p => GetReadableName(p.ParameterType)))}): {GetReadableName(returnType)}";
                var md5 = MD5.Create();
                var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
                var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                AddAction(hash, methodName, parameters.Select(p => p.ParameterType).ToArray(), returnType);
            }
        }

        private static string GetReadableName(Type type)
        {
            if (!type.IsGenericType) return type.Name;
            
            var genericArguments = type.GetGenericArguments();
            var genericName = string.Join(", ", genericArguments.Select(GetReadableName));
            var genericType = type.Name.Substring(0, type.Name.IndexOf("`", StringComparison.Ordinal));
            return $"{genericType}<{genericName}>";
        }

        protected virtual void AddAction(string key, string methodName, Type[] parameterTypes, Type returnType)
        {
            AvailableFunctions[key] = args =>
            {
                var method = typeof(IpcInterfaceBridge<TInterface>)
                    .GetMethod(nameof(InvokeMethodOverIpcAsync), BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.MakeGenericMethod(returnType);

                Debug.Assert(method != null);

                // Calls InvokeMethodOverIpcAsync<returnType>(key, args) and returns Task<object>
                return method.Invoke(this, new object[] { key, args });
            };
        }

        protected async Task<TReturnType> InvokeMethodOverIpcAsync<TReturnType>(string hash, object[] parameters)
        {
            var (requestPipe, responsePipe) = Transport.GetPipePaths(PipeName);
            var payload = JsonConvert.SerializeObject(new { Hash = hash, Params = parameters });
            await Transport.WriteAsync(requestPipe, payload);
            var resultJson = await Transport.ReadAsync(responsePipe);

            var resultType = typeof(TReturnType);
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var innerType = resultType.GetGenericArguments()[0];
                var deserialized = JsonConvert.DeserializeObject(resultJson, innerType);

                var fromResultMethod = typeof(Task)
                    .GetMethod(nameof(Task.FromResult));
                
                Debug.Assert(fromResultMethod != null);
                
                // Create Task.FromResult<T>(deserialized) and cast to TReturnType
                var taskFromResult = fromResultMethod
                    .MakeGenericMethod(innerType)
                    .Invoke(null, new[] { deserialized });

                return (TReturnType)taskFromResult;
            }

            var result = JsonConvert.DeserializeObject<TReturnType>(resultJson);
            return result;
        }

        public async Task<object> ExecuteFunctionAsync(string hash, params object[] parameters)
        {
            try
            {
                if (!AvailableFunctions.TryGetValue(hash, out var func))
                    throw new InvalidOperationException("Function not found for signature: " + hash);

                var result = func(parameters);

                if (!(result is Task taskResult)) return result;

                await taskResult;

                var taskType = taskResult.GetType();
                Debug.Assert(taskType.IsGenericType);

                var resultProperty = taskType.GetProperty("Result");
                Debug.Assert(resultProperty != null);

                return resultProperty.GetValue(taskResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing function: {hash} - {ex.Message}");
                return null;
            }
        }
    }
}