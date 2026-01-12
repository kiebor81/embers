using Embers.Language;

namespace Embers.Expressions
{
    /// <summary>
    /// LambdaExpression creates a Proc object from a block expression.
    /// This allows blocks to be first-class objects that can be assigned to variables.
    /// Example: f = lambda { |x| x * 2 }
    /// </summary>
    public class LambdaExpression(BlockExpression block) : BaseExpression
    {
        private readonly BlockExpression block = block;

        public override object Evaluate(Context context)
        {
            // Create a Block object with the current context (closure)
            var blockObj = new Block(
                block.Parameters,
                block.Body,
                context
            );

            // Return a Proc object wrapping the block
            return new Proc(blockObj);
        }

        public override bool Equals(object? obj)
        {
            return obj is LambdaExpression other && block.Equals(other.block);
        }

        public override int GetHashCode()
        {
            return typeof(LambdaExpression).GetHashCode() ^ block.GetHashCode();
        }
    }
}
