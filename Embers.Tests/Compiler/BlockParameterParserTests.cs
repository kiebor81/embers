using Embers.Compiler;
using Embers.Expressions;

namespace Embers.Tests.Compiler
{
    [TestClass]
    public class BlockParameterParserTests
    {
        [TestMethod]
        public void TestParseBlockParameterOnly()
        {
            var parser = new Parser("def foo(&block); end");
            var expr = parser.ParseExpression() as DefExpression;
            
            Assert.IsNotNull(expr);
            Assert.AreEqual(0, expr.Parameters.Count, "Should have 0 regular parameters");
            Assert.AreEqual("block", expr.BlockParameterName, "Should have block parameter named 'block'");
        }

        [TestMethod]
        public void TestParseBlockParameterWithRegularParams()
        {
            var parser = new Parser("def bar(a, b, &block); end");
            var expr = parser.ParseExpression() as DefExpression;
            
            Assert.IsNotNull(expr);
            Assert.AreEqual(2, expr.Parameters.Count, "Should have 2 regular parameters");
            Assert.AreEqual("a", expr.Parameters[0]);
            Assert.AreEqual("b", expr.Parameters[1]);
            Assert.AreEqual("block", expr.BlockParameterName, "Should have block parameter named 'block'");
        }
    }
}
