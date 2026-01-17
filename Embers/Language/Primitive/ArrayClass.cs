using System.Collections;

namespace Embers.Language.Primitive;

/// <summary>
/// ArrayClass represents the Array class in the Embers language.
/// </summary>
/// <seealso cref="NativeClass" />
public class ArrayClass : NativeClass
{
    public ArrayClass(Machine machine)
        : base("Array", machine)
    {
        SetInstanceMethod("each", DoEach);
    }

    private static object DoEach(object obj, IList<object> values)
    {
        var block = (Block)values[0];
        IList list = (IList)obj;

        foreach (var value in list)
            block.Apply([value]);

        return obj;
    }
}

