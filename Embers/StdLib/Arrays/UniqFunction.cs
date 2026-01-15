using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Arrays;

[StdLib("uniq", TargetType = "Array")]
public class UniqFunction : StdFunction
{
    [Comments("Returns a new array with duplicate elements removed.")]
    [Arguments(ParamNames = new[] { "array_data" }, ParamTypes = new[] { typeof(Array) })]
    [Returns(ReturnType = typeof(Array))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("uniq expects an array argument");

        if (values[0] is IEnumerable arr)
        {
            var result = new DynamicArray();
            foreach (var item in arr.Cast<object>().Distinct())
                result.Add(item);
            return result;
        }

        throw new TypeError("uniq expects an array");
    }
}

