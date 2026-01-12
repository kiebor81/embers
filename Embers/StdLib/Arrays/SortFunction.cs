using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Arrays
{
    /// <summary>
    /// Returns a sorted copy of the array.
    /// </summary>
    [StdLib("sort", TargetType = "Array")]
    public class SortFunction : StdFunction
    {
        [Comments("Returns a sorted copy of the array.")]
        [Arguments(ParamNames = new[] { "array_data" }, ParamTypes = new[] { typeof(Array) })]
        [Returns(ReturnType = typeof(Array))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("sort expects an array argument");

            if (values[0] is IEnumerable arr)
            {
                var result = new DynamicArray();
                foreach (var item in arr.Cast<object>().OrderBy(x => x))
                    result.Add(item);
                return result;
            }

            throw new TypeError("sort expects an array");
        }
    }
}

