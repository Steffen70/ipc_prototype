using System.Threading.Tasks;

namespace SwissPension.IpcPrototype.Common
{
    public class TestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class NestedTestClass
    {
        public string GroupName { get; set; }
        public TestClass TestClass { get; set; }
    }

    public interface IDemoLibrary
    {
        string SayHello(string name);
        long Add(long a, long b);
        TestClass CreateTestClass();
        NestedTestClass CreateNestedTestClass();
        Task<long> PrintMessage(string message);
        Task<long> DoSomething();
    }
}