using Embers.Annotations;
using Embers.Language;

namespace Embers.Functions
{
    /// <summary>
    /// A simple IFunction wrapper for delegate-based logic.
    /// Useful for adapting raw blocks or host functions dynamically.
    /// </summary>
    [ScannerIgnore]
    public class DelegateFunction(Func<DynamicObject, Context, IList<object>, object> function) : IFunction
    {
        private readonly Func<DynamicObject, Context, IList<object>, object> function = function ?? throw new ArgumentNullException(nameof(function));

        public object Apply(DynamicObject self, Context context, IList<object> values)
        {
            return function(self, context, values);
        }
    }
}
