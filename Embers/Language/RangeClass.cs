using System.Collections;

namespace Embers.Language;
/// <summary>
/// RangeClass represents a range of values in the runtime interpreter.
/// </summary>
/// <seealso cref="NativeClass" />
public class RangeClass : NativeClass
{
    public RangeClass(Machine machine)
        : base("Range", machine)
    {
        SetInstanceMethod("each", DoEach);
    }

    private static object DoEach(object obj, IList<object> values)
    {
        var block = (Block)values[0];
        IEnumerable list = (IEnumerable)obj;

        foreach (var value in list)
            block.Apply([value]);

        return obj;
    }
}

