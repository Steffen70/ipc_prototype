using System;
using System.Threading.Tasks;
using SwissPension.IpcPrototype.Common;

namespace SwissPension.IpcPrototype.Library
{
    public class DemoLibrary : IDemoLibrary
    {
        public string SayHello(string name) => $"Hello, {name}!";
        public long Add(long a, long b) => a + b;

        public TestClass CreateTestClass() => new TestClass()
        {
            Name = "Steffen70",
            Age = 24
        };

        public NestedTestClass CreateNestedTestClass() => new NestedTestClass()
        {
            GroupName = "Developers",
            TestClass = CreateTestClass()
        };

        public async Task<long> PrintMessage(string message)
        {
            await Task.Delay(2000);
            Console.WriteLine(message);
            return 1;
        }

        public async Task<long> DoSomething()
        {
            await Task.Delay(2000);
            Console.WriteLine("Action: Do something");
            return 1;
        }
    }
}