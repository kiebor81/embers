using Embers.Host;
using Embers.Language;
using Embers.Annotations;

namespace Embers.Console.Injected;

[HostFunction("hello")]
internal class HelloFunction : HostFunction
{
    [Comments("Prints a hello message to the console.")]
    [Returns(ReturnType = typeof(void))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        System.Console.WriteLine("Hello from Embers.Console!");
        return null;
    }
}
