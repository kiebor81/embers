using Embers.Host;
using Embers.Language;
using Embers.Annotations;

namespace Embers.Console.Injected;

[HostFunction("help")]
internal class HelpFunction : HostFunction
{
    [Comments("Displays help information for available functions.")]
    [Arguments(ParamNames = new[] { "functionName" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(void))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {

        System.Console.WriteLine("--- Start of Help ---");

        System.Console.WriteLine("");

        var documentation_dict = FunctionScanner.ScanFunctionDocumentation();

        IEnumerable<string> keysToDisplay;
        if (values == null || values.Count == 0)
        {
            keysToDisplay = documentation_dict.Keys;
        }
        else
        {
            var lookupKey = values[0].ToString();
            keysToDisplay = documentation_dict.Keys.Where(k => 
            {
                var keyParts = k.Split(',').Select(p => p.Trim());
                return keyParts.Contains(lookupKey);
            });
        }

        foreach (var key in keysToDisplay)
        {
            System.Console.WriteLine($"Method: {key}");
            var (Comments, Arguments, Returns) = documentation_dict[key];
            
            if (!string.IsNullOrEmpty(Comments))
                System.Console.WriteLine($"  Comments: {Comments}");
            
            if (!string.IsNullOrEmpty(Arguments))
                System.Console.WriteLine($"  Arguments: {Arguments}");
            
            if (!string.IsNullOrEmpty(Returns))
                System.Console.WriteLine($"  Returns: {Returns}");

            System.Console.WriteLine("");

        }

        System.Console.WriteLine("--- End of Help ---");

        return null;
    }
}