using Embers.Exceptions;
using Embers.Expressions;
using Embers.Functions;
using Embers.Language;
using Embers.Tests.Classes;

namespace Embers.Tests.Expressions
{
    [TestClass]
    public class AssignDotExpressionTests
    {
        [TestMethod]
        public void CreateAssignDotCommand()
        {
            DotExpression leftvalue = (DotExpression)(new Parser("a.b")).ParseExpression();
            IExpression value = new ConstantExpression(1);
            AssignDotExpressions cmd = new(leftvalue, value);

            Assert.AreSame(leftvalue, cmd.LeftValue);
            Assert.AreSame(value, cmd.Expression);
        }

        [TestMethod]
        public void ExecuteAssignDotCommand()
        {
            Machine machine = new();
            var @class = new DynamicClass("Dog");
            var method = new DefinedFunction((new Parser("@name = name")).ParseCommand(), ["name"], machine.RootContext);
            @class.SetInstanceMethod("name=", method);
            var nero = @class.CreateInstance();
            machine.RootContext.SetLocalValue("nero", nero);
            var leftvalue = (DotExpression)(new Parser("nero.name")).ParseExpression();
            var value = new ConstantExpression("Nero");
            AssignDotExpressions cmd = new(leftvalue, value);

            var result = cmd.Evaluate(machine.RootContext);

            Assert.IsNotNull(result);
            Assert.AreEqual("Nero", result);
            Assert.AreEqual("Nero", nero.GetValue("name"));
        }

        [TestMethod]
        public void ExecuteAssignDotCommandWithUnknownMethod()
        {
            Machine machine = new();
            var @class = new DynamicClass("Dog");
            var nero = @class.CreateInstance();
            machine.RootContext.SetLocalValue("nero", nero);
            var leftvalue = (DotExpression)(new Parser("nero.name")).ParseExpression();
            var value = new ConstantExpression("Nero");
            AssignDotExpressions cmd = new(leftvalue, value);

            try
            {
                cmd.Evaluate(machine.RootContext);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(NoMethodError));
            }
        }

        [TestMethod]
        public void ExecuteAssignDotCommandOnNativeProperty()
        {
            Person person = new();
            Machine machine = new();
            machine.RootContext.SetLocalValue("p", person);
            var leftvalue = (DotExpression)(new Parser("p.FirstName")).ParseExpression();
            var value = new ConstantExpression("Kieran");
            AssignDotExpressions cmd = new(leftvalue, value);

            var result = cmd.Evaluate(machine.RootContext);

            Assert.IsNotNull(result);
            Assert.AreEqual("Kieran", result);
            Assert.AreEqual("Kieran", person.FirstName);
        }

        [TestMethod]
        public void Equals()
        {
            DotExpression expr1 = (DotExpression)(new Parser("a.b")).ParseExpression();
            DotExpression expr2 = (DotExpression)(new Parser("a.c")).ParseExpression();

            AssignDotExpressions cmd1 = new(expr1, new ConstantExpression(1));
            AssignDotExpressions cmd2 = new(expr1, new ConstantExpression(2));
            AssignDotExpressions cmd3 = new(expr2, new ConstantExpression(1));
            AssignDotExpressions cmd4 = new(expr1, new ConstantExpression(1));

            Assert.IsTrue(cmd1.Equals(cmd4));
            Assert.IsTrue(cmd4.Equals(cmd1));
            Assert.AreEqual(cmd1.GetHashCode(), cmd4.GetHashCode());

            Assert.IsFalse(cmd1.Equals(null));
            Assert.IsFalse(cmd1.Equals(cmd2));
            Assert.IsFalse(cmd1.Equals(cmd3));
            Assert.IsFalse(cmd1.Equals(123));
        }
    }
}
