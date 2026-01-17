using Crayon;
using Embers.Host;
using Embers.Annotations;
using Embers.Language.Dynamic;

namespace Embers.Console.Injected;

[HostFunction("info")]
internal class BlueFunction : HostFunction
{
    [Comments("Prints the given input to the console in blue color.")]
    [Arguments(ParamNames = new[] { "input" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(void))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        foreach (var value in values)
            System.Console.Write(Output.Blue(value.ToString()));

        System.Console.WriteLine("");

        return null;
    }
}
