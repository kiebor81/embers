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

    /// <summary>
    /// Implements the 'each' method for Range objects.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    /// <exception cref="TypeError"></exception>
    private static object DoEach(object obj, IList<object> values)
    {
        var block = (Block)values[0];
        var range = obj as Range ?? throw new TypeError("range must be a Range");

        foreach (var value in range.Enumerate())
            block.Apply([value]);

        return obj;
    }
}


