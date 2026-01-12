using Embers.Host;
using Embers.Language;

namespace Embers.Console.Injected;

[HostFunction("help")]
internal class HelpFunction : HostFunction
{
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {

        System.Console.WriteLine("--- Start of Help ---");

        System.Console.WriteLine("");

        var documentation_dict = Annotations.FunctionScanner.ScanFunctionDocumentation();

        foreach (var key in documentation_dict.Keys)
        {
            System.Console.WriteLine($"Method: {key}");
            var doc = documentation_dict[key];
            
            if (!string.IsNullOrEmpty(doc.Comments))
                System.Console.WriteLine($"  Comments: {doc.Comments}");
            
            if (!string.IsNullOrEmpty(doc.Arguments))
                System.Console.WriteLine($"  Arguments: {doc.Arguments}");
            
            if (!string.IsNullOrEmpty(doc.Returns))
                System.Console.WriteLine($"  Returns: {doc.Returns}");

            System.Console.WriteLine("");

        }

        System.Console.WriteLine("--- End of Help ---");

        return null;
    }
}