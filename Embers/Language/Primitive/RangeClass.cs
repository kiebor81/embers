using Embers.Exceptions;

namespace Embers.Language.Primitive;
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
        var range = obj as Range ?? throw new TypeError("range must be a Range");

        foreach (var value in range.Enumerate())
            block.Apply([value]);

        return obj;
    }
}


