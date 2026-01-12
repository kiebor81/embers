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