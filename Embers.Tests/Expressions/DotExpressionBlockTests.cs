namespace Embers.Tests.Expressions
{
    [TestClass]
    public class DotExpressionBlockTests
    {
        [TestMethod]
        public void EvaluateDotExpressionWithBlock()
        {
            // Test that DotExpression correctly passes block context to methods
            Machine machine = new();
            
            // Define a simple method that accepts a block
            var code = @"
                class Counter
                    def initialize
                        @count = 0
                    end
                    
                    def times
                        i = 0
                        result = nil
                        while i < 3
                            result = yield i
                            i = i + 1
                        end
                        result
                    end
                end
                
                c = Counter.new
                c.times { |x| x * 2 }
            ";
            
            var result = machine.ExecuteText(code);
            
            // The times method should return the last yield result
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result); // Last iteration: 2 * 2 = 4
        }

        [TestMethod]
        public void EvaluateDotExpressionWithBlockAndArguments()
        {
            // Test DotExpression with both arguments and a block
            Machine machine = new();
            
            // Simple test: define a method that yields and check it works
            var code = @"
                class Counter
                    def count_to(n)
                        i = 0
                        total = 0
                        while i < n
                            total = total + (yield i)
                            i = i + 1
                        end
                        total
                    end
                end
                
                c = Counter.new
                c.count_to(3) { |x| x + 1 }
            ";
            
            var result = machine.ExecuteText(code);
            
            // Should be 1 + 2 + 3 = 6
            Assert.IsNotNull(result);
            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public void EvaluateParserBlockIntegration()
        {
            // Test the parsing and evaluation of k.times { print 'foo' }
            Machine machine = new();
            StringWriter writer = new();
            machine.RootContext.Self.Class.SetInstanceMethod("print", new Embers.StdLib.PutsFunction(writer));
            
            var code = @"
                class Counter
                    def times
                        i = 0
                        while i < 3
                            yield
                            i = i + 1
                        end
                    end
                end
                
                k = Counter.new
                k.times { print 'foo' }
            ";
            
            machine.ExecuteText(code);
            
            // Should have printed 'foo' 3 times
            var output = writer.ToString();
            Assert.AreEqual("foo\r\nfoo\r\nfoo\r\n", output);
        }

        [TestMethod]
        public void EvaluateDefinedFunctionWithBlock()
        {
            // Test that DefinedFunction works with blocks through DotExpression
            Machine machine = new();
            
            var code = @"
                class MyClass
                    def with_block
                        yield 42
                    end
                end
                
                obj = MyClass.new
                result = obj.with_block { |x| x + 8 }
            ";
            
            var result = machine.ExecuteText(code);
            var resultValue = machine.RootContext.GetValue("result");
            
            Assert.IsNotNull(resultValue);
            Assert.AreEqual(50, resultValue);
        }
    }
}
