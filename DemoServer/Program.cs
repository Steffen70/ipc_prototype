using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SwissPension.IpcInterfaceBridge;
using SwissPension.IpcPrototype.Common;

namespace SwissPension.IpcPrototype.Server
{
    internal static class Program
    {
        public static void Main()
        {
            var ipcInterfaceBridge = new IpcInterfaceBridge<IDemoLibrary>();

            var sayHello = ipcInterfaceBridge.ExecuteFunctionAsync("690F8411C49BF7132480EE44A052BD52", "Steffen70").Result;
            
            Console.WriteLine($"SayHello() => \"{sayHello}\"");

            var add = ipcInterfaceBridge.ExecuteFunctionAsync("E7CA457820BC46F316A670A616C3807B", 1 , 5).Result;
            
            Console.WriteLine($"Add(1, 5) => {add}");
            
            var createTestClass = ipcInterfaceBridge.ExecuteFunctionAsync("4FD7A9D45BDD6D59A2D842E100F68CA3").Result;
            var createTestClassJson = JsonConvert.SerializeObject(createTestClass);
            
            Console.WriteLine($"CreateTestClass() => {createTestClassJson}");
            
            var createNestedTestClass = ipcInterfaceBridge.ExecuteFunctionAsync("CAA4C38FD95732EF97875DE669F94C60").Result;
            var createNestedTestClassJson = JsonConvert.SerializeObject(createNestedTestClass);
            
            Console.WriteLine($"CreateNestedTestClass() => {createNestedTestClassJson}");
            
            var printMessage = ipcInterfaceBridge.ExecuteFunctionAsync("8DC7F63F3480B2DD6E2C7A5EED24FB68", "Test Message").Result;
            var printMessageResult = ((Task<long>)printMessage).Result;
            
            Console.WriteLine($"PrintMessage(\"Test Message\") => {printMessageResult}");
            
            var doSomething = ipcInterfaceBridge.ExecuteFunctionAsync("39D54F5CE9DA81EBEB9812FDDBCC2189").Result;
            var doSomethingResult = ((Task<long>)doSomething).Result;
            
            Console.WriteLine($"DoSomething() => {doSomethingResult}");
        }
    }
}