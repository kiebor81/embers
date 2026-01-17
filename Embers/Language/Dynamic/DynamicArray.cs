using Embers.StdLib;
using System.Collections;

namespace Embers.Language.Dynamic;
/// <summary>
/// DynamicArray represents a dynamic array in the Embers language.
/// </summary>
public class DynamicArray : ArrayList
{
    public override object this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                return null;

            return base[index];
        }

        set
        {
            while (index >= Count)
                Add(null);

            base[index] = value;
        }
    }

    public override string ToString()
    {
        var result = "[";

        foreach (var value in this)
        {
            if (result.Length > 1)
                result += ", ";

            if (value == null)
                result += "nil";
            else
                result += value.ToString();
        }

        result += "]";

        return result;
    }

    /// <summary>
    /// Gets a method for this array by name.
    /// Uses StdLibRegistry for automatic discovery of array-specific methods.
    /// </summary>
    public object? GetMethod(string name)
    {
        return StdLibRegistry.GetMethod("Array", name);
    }
}

