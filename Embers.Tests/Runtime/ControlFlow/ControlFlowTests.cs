using Embers.Exceptions;
using Embers.Expressions;
using Embers.Host;
using Embers.StdLib;

namespace Embers.Tests
{
    [TestClass]
    public class ControlFlowTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        [TestMethod]
        public void EvaluateRaiseThrowsException()
        {
            try
            {
                EvaluateExpression("raise 'fail!'");
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("fail!"));
            }
        }

        [TestMethod]
        public void EvaluateRaiseWithTypeAndMessage()
        {
            try
            {
                EvaluateExpression("raise ArgumentError, 'Invalid argument!'");
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentError));
                Assert.IsTrue(ex.Message.Contains("Invalid argument!"));
            }
        }

        [TestMethod]
        public void EvaluatePostfixIfUnless()
        {
            var output = new StringWriter();
            var localMachine = new Machine();
            localMachine.RootContext.Self.Class.SetInstanceMethod("puts", new PutsFunction(output));

            localMachine.ExecuteText(@"puts 'monkey' unless 3 > 4");
            Assert.AreEqual("monkey\n", output.ToString().Replace("\r\n", "\n"));

            output.GetStringBuilder().Clear();
            localMachine.ExecuteText(@"puts 'nope' unless 3 < 4");
            Assert.AreEqual("", output.ToString().Trim());

            output.GetStringBuilder().Clear();
            localMachine.ExecuteText(@"puts 'banana' if 3 < 4");
            Assert.AreEqual("banana\n", output.ToString().Replace("\r\n", "\n"));

            output.GetStringBuilder().Clear();
            localMachine.ExecuteText(@"puts 'nope' if 3 > 4");
            Assert.AreEqual("", output.ToString().Trim());
        }

        [TestMethod]
        public void EvaluateTernaryOperator()
        {
            Assert.AreEqual("yes", EvaluateExpression("true ? 'yes' : 'no'"));
            Assert.AreEqual("no", EvaluateExpression("false ? 'yes' : 'no'"));
            Assert.AreEqual(42L, EvaluateExpression("1 == 1 ? 42 : 0"));
            Assert.AreEqual(0L, EvaluateExpression("1 != 1 ? 42 : 0"));
        }

        [TestMethod]
        public void ParseAndOrExpressions()
        {
            Assert.AreEqual(true, EvaluateExpression("true && true"));
            Assert.AreEqual(false, EvaluateExpression("true && false"));
            Assert.AreEqual(true, EvaluateExpression("false || true"));
            Assert.AreEqual(false, EvaluateExpression("false || false"));

            // Short-circuit tests
            Assert.AreEqual(false, EvaluateExpression("false && raise 'fail'"));
            Assert.AreEqual(true, EvaluateExpression("true || raise 'fail'"));

            Assert.AreEqual(false, EvaluateExpression("false and raise 'fail'"));
            Assert.AreEqual(true, EvaluateExpression("true or raise 'fail'"));
        }

        [TestMethod]
        public void LoopControlFlow()
        {
            Assert.AreEqual(6L, Execute(@"
total = 0
for i in 1..5 do
    if i < 4 then
        total = total + i
    else
        break i
    end
end
total
            "));

            Assert.AreEqual(9L, Execute(@"
total = -1
for i in 1..10 do
    if i < 10 then
        next
    end
    total = total + i
end
total
            "));

            Assert.AreEqual(5L, Execute(@"
i = 0
while i < 5
    i = i + 1
    redo if i == 3
end
i
            "));
        }

        [TestMethod]
        public void ReturnExitsMethodEarly()
        {
            var result = Execute(@"
def foo
  return 1
  2
end
foo
");

            Assert.AreEqual(1L, result);
        }

        [TestMethod]
        public void ReturnWithoutValueReturnsNil()
        {
            var result = Execute(@"
def foo
  return
  2
end
foo
");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ReturnAtTopLevelRaises()
        {
            try
            {
                Execute("return 1");
                Assert.Fail("Expected InvalidOperationError");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationError));
            }
        }

        [TestMethod]
        public void ReturnInsideBlockRaises()
        {
            try
            {
                Execute(@"
def foo
  [1].each do |x|
    return x
  end
  2
end
foo
");
                Assert.Fail("Expected InvalidOperationError");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationError));
            }
        }

        [TestMethod]
        public void ModuloAndPowerOperators()
        {
            Assert.AreEqual(1L, EvaluateExpression("5 % 2"));
            Assert.AreEqual(0L, EvaluateExpression("4 % 2"));

            Assert.AreEqual(8L, EvaluateExpression("2 ** 3"));
            Assert.AreEqual(1L, EvaluateExpression("10 % 3 % 3"));
            Assert.AreEqual(81L, EvaluateExpression("3 ** 2 ** 2"));
        }

        [TestMethod]
        public void NotKeywordWorks()
        {
            Assert.AreEqual(false, EvaluateExpression("not true"));
            Assert.AreEqual(true, EvaluateExpression("not false"));
            Assert.AreEqual(false, EvaluateExpression("not 1 == 1"));
        }

        [TestMethod]
        public void AndNotWithUserPredicateCalls()
        {
            var result = Execute(@"
def player_is_colour?(value)
  value == 'green'
end

def player_has_item?(key, count)
  false
end

triggered = false
if player_is_colour? 'green' and not player_has_item?('KEY1', 3)
  triggered = true
end
triggered
");

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void AndNotWithStdLibPredicateCalls()
        {
            var result = Execute(@"
triggered = false
if 'green'.include?('gr') and not 'green'.include?('zz')
  triggered = true
end
triggered
");

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void AndNotWithHostPredicateCalls()
        {
            var localMachine = new Machine();
            localMachine.InjectFromAssembly(typeof(MyLib.MyClass).Assembly);

            var result = localMachine.ExecuteText(@"
triggered = false
if host_is_colour? 'green' and not host_has_item?('KEY1', 3)
  triggered = true
end
triggered
");

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void AndWithComparisonsInIf()
        {
            var result = Execute(@"
thing = 'green'
other_thing = 'blue'
triggered = false
if thing == 'green' and other_thing == 'blue'
  triggered = true
end
triggered
");

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void NotWithComparisonInIf()
        {
            var result = Execute(@"
thing = 'blue'
triggered = false
if not thing == 'green'
  triggered = true
end
triggered
");

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void ElsifWorks() => Assert.AreEqual("middle", Execute(@"
if false
  'low'
elsif true
  'middle'
else
  'high'
end
"));

        [TestMethod]
        public void DefinedKeywordDetectsLocals()
        {
            Assert.AreEqual("local-variable", Execute(@"
foo = 123
defined? foo
    "));

            Assert.AreEqual(null, Execute(@"
defined? bar
    "));
        }

        [TestMethod]
        public void YieldCallsBlockWithoutArgs()
        {
            var result = Execute(@"
def run
  yield
end

run { 'ok' }
");
            Assert.AreEqual("ok", result);
        }

        [TestMethod]
        public void YieldPassesArgumentsToBlock()
        {
            var result = Execute(@"
def double(x)
  yield x * 2
end

double(3) { |n| n + 1 }  # Should yield 6 -> 7
");
            Assert.AreEqual(7L, result);
        }

        [TestMethod]
        public void YieldWithoutBlockRaises()
        {
            try
            {
                Execute(@"
def run
  yield
end

run
");
                Assert.Fail("Expected NameError");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(NameError));
                Assert.IsTrue(ex.Message.Contains("no block given"));
            }
        }

        [TestMethod]
        public void MultipleYields()
        {
            var result = Execute(@"
def tally
  yield
  yield
end

count = 0
puts count

tally { count += 1 }
count
");
            Assert.AreEqual(2L, result);
        }

        private object EvaluateExpression(string text)
        {
            Parser parser = new(text);
            IExpression expression = parser.ParseExpression();
            Assert.IsNull(parser.ParseExpression());
            return expression.Evaluate(machine.RootContext);
        }

        private object Execute(string text) => machine.ExecuteText(text);
    }
}
