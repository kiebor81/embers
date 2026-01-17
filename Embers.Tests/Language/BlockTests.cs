using Embers.Expressions;
using Embers.Language.Primitive;

namespace Embers.Tests.Language
{
    [TestClass]
    public class BlockTests
    {
        [TestMethod]
        public void CreateAndEvaluateSimpleBlock()
        {
            Block block = new(null, new ConstantExpression(1), null);

            Assert.AreEqual(1, block.Apply(null));
        }

        [TestMethod]
        public void CreateAndEvaluateBlockWithFreeVariable()
        {
            Context context = new();
            context.SetLocalValue("a", 1);
            Block block = new(null, new AddExpression(new NameExpression("a"), new ConstantExpression(1)), context);

            Assert.AreEqual(2, block.Apply(null));
        }

        [TestMethod]
        public void CreateAndEvaluateBlockWithArguments()
        {
            Block block = new(["a", "b"], new AddExpression(new NameExpression("a"), new NameExpression("b")), new Context());

            Assert.AreEqual(3, block.Apply([1, 2]));
        }

        [TestMethod]
        public void CreateAndEvaluateBlockWithNonProvidedArgument()
        {
            Context context = new();
            context.SetLocalValue("a", 1);
            Block block = new(["a"], new NameExpression("a"), context);

            Assert.IsNull(block.Apply([]));
        }
    }
}
