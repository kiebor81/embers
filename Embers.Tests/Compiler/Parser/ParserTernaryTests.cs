using Embers.Expressions;

namespace Embers.Tests.Compiler
{
    [TestClass]
    public class ParserTernaryTests
    {
        [TestMethod]
        public void ParseCallWithTernaryArgument()
        {
            Parser parser = new("puts true ? \"yes\" : \"no\"");
            var expected = new CallExpression("puts", [
                new TernaryExpression(
                    new ConstantExpression(true),
                    new ConstantExpression("yes"),
                    new ConstantExpression("no")
                )
            ]);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
        }
    }
}
