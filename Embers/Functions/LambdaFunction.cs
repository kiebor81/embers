using Embers.Annotations;
using Embers.Language;

namespace Embers.Functions
{
    /// <summary>
    /// LambdaFunction is a function that is defined by a lambda expression.
    /// </summary>
    /// <seealso cref="Embers.Functions.IFunction" />
    [ScannerIgnore]
    public class LambdaFunction(Func<DynamicObject, Context, IList<object>, object> lambda) : IFunction
    {
        private readonly Func<DynamicObject, Context, IList<object>, object> lambda = lambda;

        public object Apply(DynamicObject self, Context context, IList<object> values)
        {
            return lambda(self, context, values);
        }
    }
}
