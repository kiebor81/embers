using Embers.Compiler;
using Embers.Exceptions;
using Embers.Expressions;

namespace Embers.Tests.Expressions
{
    [TestClass]
    public class DoubleColonExpressionTests
    {
        [TestMethod]
        public void EvaluateConstant()
        {
            Machine machine = new();
            machine.ExecuteText("module MyModule; FOO = 3; end");

            DoubleColonExpression expression = new(new NameExpression("MyModule"), "FOO");

            var result = expression.Evaluate(machine.RootContext);

            Assert.IsNotNull(result);
            Assert.AreEqual(3L, result);
        }

        [TestMethod]
        public void EvaluateUndefinedConstant()
        {
            Machine machine = new();
            machine.ExecuteText("module MyModule;end");

            DoubleColonExpression expression = new(new NameExpression("MyModule"), "FOO");

            try
            {
                expression.Evaluate(machine.RootContext);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(NameError));
                Assert.AreEqual("unitialized constant MyModule::FOO", ex.Message);
            }
        }

        [TestMethod]
        public void EvaluateTypeConstant()
        {
            Machine machine = new();

            DoubleColonExpression expression = new(new ConstantExpression(typeof(TokenType)), "Name");

            var result = expression.Evaluate(machine.RootContext);

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenType.Name, result);
        }

        [TestMethod]
        public void GetLocalVariables()
        {
            DoubleColonExpression expression = new(new ConstantExpression(typeof(TokenType)), "Name");
            Assert.IsNull(expression.GetLocalVariables());
        }

        [TestMethod]
        public void NamedExpression()
        {
            INamedExpression expression = new DoubleColonExpression(new ConstantExpression(1), "name");

            Assert.AreEqual(new ConstantExpression(1), expression.TargetExpression);
            Assert.AreEqual("name", expression.Name);
        }

        [TestMethod]
        public void Equals()
        {
            DoubleColonExpression expr1 = new(new ConstantExpression(1), "foo");
            DoubleColonExpression expr2 = new(new ConstantExpression(2), "foo");
            DoubleColonExpression expr3 = new(new ConstantExpression(1), "bar");
            DoubleColonExpression expr4 = new(new ConstantExpression(1), "foo");

            Assert.IsTrue(expr1.Equals(expr4));
            Assert.IsTrue(expr4.Equals(expr1));
            Assert.AreEqual(expr1.GetHashCode(), expr4.GetHashCode());

            Assert.IsFalse(expr1.Equals(expr2));
            Assert.IsFalse(expr2.Equals(expr1));
            Assert.IsFalse(expr1.Equals(expr3));
            Assert.IsFalse(expr3.Equals(expr1));
            Assert.IsFalse(expr1.Equals(null));
            Assert.IsFalse(expr1.Equals("foo"));
        }
    }
}
