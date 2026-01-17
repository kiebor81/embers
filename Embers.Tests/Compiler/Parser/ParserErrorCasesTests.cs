using Embers.Exceptions;

namespace Embers.Tests.Compiler
{
    [TestClass]
    public class ParserErrorCasesTests
    {
        [TestMethod]
        public void BlockArgumentSymbolMissingName_ThrowsSyntaxError()
        {
            Parser parser = new("map(&:)");
            try
            {
                parser.ParseExpression();
                Assert.Fail("Expected SyntaxError");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("Expected expression after &", ex.Message);
            }
        }

        [TestMethod]
        public void StabbyLambdaWithoutBlock_ThrowsSyntaxError()
        {
            Parser parser = new("->");
            try
            {
                parser.ParseExpression();
                Assert.Fail("Expected SyntaxError");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("stabby lambda requires a block", ex.Message);
            }
        }

        [TestMethod]
        public void StabbyLambdaMissingClosingParen_ThrowsSyntaxError()
        {
            Parser parser = new("->(a");
            try
            {
                parser.ParseExpression();
                Assert.Fail("Expected SyntaxError");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("expected ')'", ex.Message);
            }
        }
    }
}
