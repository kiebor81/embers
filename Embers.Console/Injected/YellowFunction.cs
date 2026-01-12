using Crayon;
using Embers.Host;
using Embers.Language;
using Embers.Annotations;

namespace Embers.Console.Injected;

[HostFunction("warning")]
internal class YellowFunction : HostFunction
{
    [Comments("Prints the given input to the console in yellow color.")]
    [Arguments(ParamNames = new[] { "input" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(void))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        foreach (var value in values)
            System.Console.Write(Output.Yellow(value.ToString()));

        System.Console.WriteLine("");

        return null;
    }
}
