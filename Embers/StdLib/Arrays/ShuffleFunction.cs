using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Arrays;

[StdLib("shuffle", TargetType = "Array")]
public class ShuffleFunction : StdFunction
{
    private static readonly Random _random = new();

    [Comments("Returns a new array with the elements shuffled randomly.")]
    [Arguments(ParamNames = new[] { "array_data" }, ParamTypes = new[] { typeof(Array) })]
    [Returns(ReturnType = typeof(Array))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("shuffle expects an array argument");

        if (values[0] is IList arr)
        {
            var copy = new DynamicArray();
            foreach (var item in arr)
                copy.Add(item);
            int n = copy.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                (copy[n], copy[k]) = (copy[k], copy[n]);
            }
            return copy;
        }

        throw new TypeError("shuffle expects an array");
    }
}

