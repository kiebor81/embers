using Embers.Language;

namespace Embers.Functions
{
    /// <summary>
    /// BlockAdapter wraps a Block so it can be used as an IFunction.
    /// This is needed when passing a Proc created from a lambda to another method's block parameter.
    /// </summary>
    public class BlockAdapter(Block block) : IFunction
    {
        private readonly Block block = block;

        public object Apply(DynamicObject self, Context caller, IList<object> values)
        {
            return block.Apply(values);
        }
    }
}
