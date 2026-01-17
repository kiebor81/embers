using Embers.Annotations;

namespace Embers.Functions;
/// <summary>
/// RequireFunction is used to load and execute a file, or reference a module, library, or assembly.
/// It takes a filename as an argument and loads the file if it has not been loaded before.
/// It represents the `require` functionality.
/// </summary>
/// <seealso cref="IFunction" />
[ScannerIgnore]
public class RequireFunction(Machine machine) : IFunction
{
    private readonly Machine machine = machine;

    public object Apply(DynamicObject self, Context context, IList<object> values)
    {
        string filename = (string)values[0];
        return machine.RequireFile(filename);
    }
}

