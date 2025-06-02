using SwissPension.IpcPrototype.Common;

namespace SwissPension.IpcPrototype.Library
{
    public class DemoLibrary: IDemoLibrary
    {
        public string SayHello(string name)
        {
            return $"Hello, {name}!";
        }
    }
}