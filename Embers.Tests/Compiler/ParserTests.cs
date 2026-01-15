using Embers.Compiler;
using Embers.Exceptions;
using Embers.Expressions;
using Embers.Language;

namespace Embers.Tests.Compiler
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void ParseInteger()
        {
            Parser parser = new("123");
            var expected = new ConstantExpression(123L);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseName()
        {
            Parser parser = new("foo");
            var expected = new NameExpression("foo");
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseSingleQuoteString()
        {
            Parser parser = new("'foo'");
            var expected = new ConstantExpression("foo");
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseAddTwoIntegers()
        {
            Parser parser = new("1+2");
            var expected = new AddExpression(new ConstantExpression(1L), new ConstantExpression(2L));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseAddTwoIntegersInParentheses()
        {
            Parser parser = new("(1+2)");
            var expected = new AddExpression(new ConstantExpression(1L), new ConstantExpression(2L));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void RaiseIsMissingParenthesis()
        {
            Parser parser = new("(1+2");

            try
            {
                parser.ParseExpression();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("expected ')'", ex.Message);
            }
        }

        [TestMethod]
        public void ParseSubtractTwoIntegers()
        {
            Parser parser = new("1-2");
            var expected = new SubtractExpression(new ConstantExpression(1L), new ConstantExpression(2L));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseSubtractThreeIntegers()
        {
            Parser parser = new("1-2-3");
            var expected = new SubtractExpression(new SubtractExpression(new ConstantExpression(1L), new ConstantExpression(2L)), new ConstantExpression(3L));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseMultiplyTwoIntegers()
        {
            Parser parser = new("3*2");
            var expected = new MultiplyExpression(new ConstantExpression(3L), new ConstantExpression(2L));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseAddAndMultiplyIntegers()
        {
            Parser parser = new("1+3*2");
            var expected = new AddExpression(new ConstantExpression(1L), new MultiplyExpression(new ConstantExpression(3L), new ConstantExpression(2L)));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseDivideTwoIntegers()
        {
            Parser parser = new("3/2");
            var expected = new DivideExpression(new ConstantExpression(3L), new ConstantExpression(2L));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseSubtractAndDivideIntegers()
        {
            Parser parser = new("1-3/2");
            var expected = new SubtractExpression(new ConstantExpression(1L), new DivideExpression(new ConstantExpression(3L), new ConstantExpression(2L)));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseNegativeInteger()
        {
            Parser parser = new("-123");
            var expected = new NegativeExpression(new ConstantExpression(123L));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParsePositiveInteger()
        {
            Parser parser = new("+123");
            var expected = new ConstantExpression(123L);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseBooleanNegation()
        {
            Parser parser = new("!a");
            var expected = new NegationExpression(new NameExpression("a"));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseNegativeIntegerMinusInteger()
        {
            Parser parser = new("-123-10");
            var expected = new SubtractExpression(new NegativeExpression(new ConstantExpression(123L)), new ConstantExpression(10L));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseCallExpressionSimplePuts()
        {
            Parser parser = new("puts 123");
            var expected = new CallExpression("puts", [new ConstantExpression(123L)]);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseCallExpressionPutsWithTwoArguments()
        {
            Parser parser = new("puts 1,2");
            var expected = new CallExpression("puts", [new ConstantExpression(1L), new ConstantExpression(2L)]);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseCallExpressionPutsWithParentheses()
        {
            Parser parser = new("puts(123)");
            var expected = new CallExpression("puts", [new ConstantExpression(123L)]);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseCallExpressionPutsWithTwoArgumentsAndParentheses()
        {
            Parser parser = new("puts(1,2)");
            var expected = new CallExpression("puts", [new ConstantExpression(1L), new ConstantExpression(2L)]);
            
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseAddTwoNameExpressions()
        {
            Parser parser = new("foo+bar");
            var expected = new AddExpression(new NameExpression("foo"), new NameExpression("bar"));
            
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        [ExpectedException(typeof(SyntaxError))]
        public void RaiseIfNotACommand()
        {
            Parser parser = new("*");
            parser.ParseCommand();
        }

        [TestMethod]
        public void ParseSimpleAssignCommand()
        {
            Parser parser = new("a=2");
            var expected = new AssignExpression("a", new ConstantExpression(2L));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleAssignCommandWithEndOfLine()
        {
            Parser parser = new("a=2\n");
            var expected = new AssignExpression("a", new ConstantExpression(2L));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleAssignCommandPrecededByAnEndOfLine()
        {
            Parser parser = new("\na=2");
            var expected = new AssignExpression("a", new ConstantExpression(2L));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        [ExpectedException(typeof(SyntaxError))]
        public void RaiseIfEndOfCommandIsMissing()
        {
            Parser parser = new("a=2 b=3\n");

            parser.ParseCommand();
        }

        [TestMethod]
        public void ParseAssignInstanceVarCommand()
        {
            Parser parser = new("@a=2");
            var expected = new AssignInstanceVarExpression("a", new ConstantExpression(2L));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseAssignClassVarCommand()
        {
            Parser parser = new("@@a=2");
            var expected = new AssignClassVarExpression("a", new ConstantExpression(2L));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseAssignDotCommand()
        {
            Parser parser = new("a.b = 2");
            DotExpression dotexpr = (DotExpression)(new Parser("a.b")).ParseExpression();
            var expected = new AssignDotExpressions(dotexpr, new ConstantExpression(2L));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseTwoNameAsCall()
        {
            Parser parser = new("a b\n");
            var expected = new CallExpression("a", [new NameExpression("b")]);
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseExpressionCommand()
        {
            Parser parser = new("1+2");
            var expected = new AddExpression(new ConstantExpression(1L), new ConstantExpression(2L));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleNameAsExpressionCommand()
        {
            Parser parser = new("a");
            var expected = new NameExpression("a");
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleNameAndNewLineAsExpressionCommand()
        {
            Parser parser = new("a\n");
            var expected = new NameExpression("a");
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleIfCommand()
        {
            Parser parser = new("if 1\n a=1\nend");
            var expected = new IfExpression(new ConstantExpression(1L), new AssignExpression("a", new ConstantExpression(1L)));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleIfElseCommand()
        {
            Parser parser = new("if 1\n a=1\nelse\n a=2\nend");
            var expected = new IfExpression(new ConstantExpression(1L), new AssignExpression("a", new ConstantExpression(1L)), new AssignExpression("a", new ConstantExpression(2L)));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleIfElifElseCommand()
        {
            Parser parser = new("if 1\n a=1\nelif 2\n a=2\nelse\n a=3\nend");
            var innerexpected = new IfExpression(new ConstantExpression(2L), new AssignExpression("a", new ConstantExpression(2L)), new AssignExpression("a", new ConstantExpression(3L)));
            var expected = new IfExpression(new ConstantExpression(1L), new AssignExpression("a", new ConstantExpression(1L)), innerexpected);
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleIfCommandWithThen()
        {
            Parser parser = new("if 1 then\n a=1\nend");
            var expected = new IfExpression(new ConstantExpression(1L), new AssignExpression("a", new ConstantExpression(1L)));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleIfCommandWithThenOneLine()
        {
            Parser parser = new("if 1 then a=1 end");
            var expected = new IfExpression(new ConstantExpression(1L), new AssignExpression("a", new ConstantExpression(1L)));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleIfCommandWithSemicolonOneLine()
        {
            Parser parser = new("if 1; a=1 end");
            var expected = new IfExpression(new ConstantExpression(1L), new AssignExpression("a", new ConstantExpression(1L)));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleIfCommandWithThenOneLineAndOtherLine()
        {
            Parser parser = new("if 1 then a=1\nb=2 end");
            var expected = new IfExpression(new ConstantExpression(1L), new CompositeExpression([new AssignExpression("a", new ConstantExpression(1L)), new AssignExpression("b", new ConstantExpression(2L))]));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseIfCommandWithCompositeThenCommand()
        {
            Parser parser = new("if 1\n a=1\n b=2\nend");
            var expected = new IfExpression(new ConstantExpression(1L), new CompositeExpression([new AssignExpression("a", new ConstantExpression(1L)), new AssignExpression("b", new ConstantExpression(2L))]));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        [ExpectedException(typeof(SyntaxError))]
        public void RaiseIfNoEndAtIf()
        {
            Parser parser = new("if 1\n a=1\n");

            parser.ParseCommand();
        }

        [TestMethod]
        public void ParseSimpleDefCommand()
        {
            Parser parser = new("def foo\na=1\nend");
            var expected = new DefExpression(new NameExpression("foo"), [], new AssignExpression("a", new ConstantExpression(1L)));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseReturnCommand()
        {
            Parser parser = new("return 42");
            var expected = new ReturnExpression(new ConstantExpression(42L));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseReturnCommandWithoutValue()
        {
            Parser parser = new("return");
            var expected = new ReturnExpression(null);
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseDefCommandWithParameters()
        {
            Parser parser = new("def foo a, b\na+b\nend");
            var expected = new DefExpression(new NameExpression("foo"), ["a", "b"], new AddExpression(new NameExpression("a"), new NameExpression("b")));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseDefCommandWithParametersInParentheses()
        {
            Parser parser = new("def foo(a, b)\na+b\nend");
            var expected = new DefExpression(new NameExpression("foo"), ["a", "b"], new AddExpression(new NameExpression("a"), new NameExpression("b")));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void RaiseIfDefHasNoName()
        {
            Parser parser = new("def \na=1\nend");

            try
            {
                parser.ParseCommand();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("name expected", ex.Message);
            }
        }

        [TestMethod]
        public void ParseObjectDefCommand()
        {
            Parser parser = new("def a.foo\na=1\nend");
            var expected = new DefExpression(new DotExpression(new NameExpression("a"), "foo", []), [], new AssignExpression("a", new ConstantExpression(1L)));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSelfDefCommand()
        {
            Parser parser = new("def self.foo\na=1\nend");
            var expected = new DefExpression(new DotExpression(new SelfExpression(), "foo", []), [], new AssignExpression("a", new ConstantExpression(1L)));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseObjectDefCommandUsingDoubleColon()
        {
            Parser parser = new("def a::foo\na=1\nend");
            var expected = new DefExpression(new DoubleColonExpression(new NameExpression("a"), "foo"), [], new AssignExpression("a", new ConstantExpression(1L)));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSelfDefCommandUsingDoubleColon()
        {
            Parser parser = new("def self::foo\na=1\nend");
            var expected = new DefExpression(new DoubleColonExpression(new SelfExpression(), "foo"), [], new AssignExpression("a", new ConstantExpression(1L)));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSymbol()
        {
            Parser parser = new(":foo");
            var expected = new ConstantExpression(new Symbol("foo"));
            var expression = parser.ParseExpression();

            Assert.IsNotNull(expression);
            Assert.AreEqual(expected, expression);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseSimpleClassCommand()
        {
            Parser parser = new("class Dog\na=1\nend");
            var expected = new ClassExpression(new NameExpression("Dog"), new AssignExpression("a", new ConstantExpression(1L)));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseQualifiedClassCommand()
        {
            Parser parser = new("class Animals::Dog\na=1\nend");
            var expected = new ClassExpression(new DoubleColonExpression(new NameExpression("Animals"), "Dog"), new AssignExpression("a", new ConstantExpression(1L)));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseDotExpression()
        {
            Parser parser = new("dog.foo");
            var expected = new DotExpression(new NameExpression("dog"), "foo", []);

            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseDotExpressionWithIntegerArgument()
        {
            Parser parser = new("dog.foo 1");
            var expected = new DotExpression(new NameExpression("dog"), "foo", [new ConstantExpression(1L)]);

            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseDotExpressionWithIntegerArgumentInParentheses()
        {
            Parser parser = new("dog.foo(1)");
            var expected = new DotExpression(new NameExpression("dog"), "foo", [new ConstantExpression(1L)]);

            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseDotExpressionWithTwoArgumentsInParentheses()
        {
            Parser parser = new("dog.foo('foo', 'bar')");
            var expected = new DotExpression(new NameExpression("dog"), "foo", [new ConstantExpression("foo"), new ConstantExpression("bar")]);

            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseDotExpressionWithIntegerAsTarget()
        {
            Parser parser = new("1.foo");
            var expected = new DotExpression(new ConstantExpression(1L), "foo", []);

            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseInstanceVariableExpression()
        {
            Parser parser = new("@a");
            var expected = new InstanceVarExpression("a");

            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseClassVariableExpression()
        {
            Parser parser = new("@@a");
            var expected = new ClassVarExpression("a");

            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseCompareExpressions()
        {
            Parser parser = new("1==2 1!=2 1<2 1>2 1<=2 1>=2");
            var expected = new IExpression[] 
            {
                new CompareExpression(new ConstantExpression(1L), new ConstantExpression(2L), CompareOperator.Equal),
                new CompareExpression(new ConstantExpression(1L), new ConstantExpression(2L), CompareOperator.NotEqual),
                new CompareExpression(new ConstantExpression(1L), new ConstantExpression(2L), CompareOperator.Less),
                new CompareExpression(new ConstantExpression(1L), new ConstantExpression(2L), CompareOperator.Greater),
                new CompareExpression(new ConstantExpression(1L), new ConstantExpression(2L), CompareOperator.LessOrEqual),
                new CompareExpression(new ConstantExpression(1L), new ConstantExpression(2L), CompareOperator.GreaterOrEqual)
            };

            foreach (var exp in expected)
                Assert.AreEqual(exp, parser.ParseExpression());

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseWhileCommand()
        {
            Parser cmdparser = new("a = a + 1");
            IExpression body = cmdparser.ParseCommand();
            Parser exprparser = new("a < 6");
            IExpression expr = exprparser.ParseExpression();

            Parser parser = new("while a<6; a=a+1; end");
            IExpression expected = new WhileExpression(expr, body);

            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseWhileCommandWithDo()
        {
            Parser cmdparser = new("a = a + 1");
            IExpression body = cmdparser.ParseCommand();
            Parser exprparser = new("a < 6");
            IExpression expr = exprparser.ParseExpression();

            Parser parser = new("while a<6 do a=a+1; end");
            IExpression expected = new WhileExpression(expr, body);

            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseUntilCommand()
        {
            Parser cmdparser = new("a = a + 1");
            IExpression body = cmdparser.ParseCommand();
            Parser exprparser = new("a >= 6");
            IExpression expr = exprparser.ParseExpression();

            Parser parser = new("until a>=6; a=a+1; end");
            IExpression expected = new UntilExpression(expr, body);

            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseUntilCommandWithDo()
        {
            Parser cmdparser = new("a = a + 1");
            IExpression body = cmdparser.ParseCommand();
            Parser exprparser = new("a >= 6");
            IExpression expr = exprparser.ParseExpression();

            Parser parser = new("until a>=6 do a=a+1; end");
            IExpression expected = new UntilExpression(expr, body);

            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleList()
        {
            Parser parser = new("[1,2,3]");
            IExpression expected = new ArrayExpression([new ConstantExpression(1L), new ConstantExpression(2L), new ConstantExpression(3L)]);

            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseSimpleListWithExpression()
        {
            Parser parser = new("[1,1+1,3]");
            IExpression expected = new ArrayExpression([new ConstantExpression(1L), new AddExpression(new ConstantExpression(1L), new ConstantExpression(1L)), new ConstantExpression(3L)]);

            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseExpression());
        }

        [TestMethod]
        public void ParseSimpleForInCommand()
        {
            Parser parser = new("for k in [1,2,3]\n puts k\nend");
            var expected = new ForInExpression("k", new ArrayExpression([new ConstantExpression(1L), new ConstantExpression(2L), new ConstantExpression(3L)]), new CallExpression("puts", [new NameExpression("k")]));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleForInCommandWithDo()
        {
            Parser parser = new("for k in [1,2,3] do\n puts k\nend");
            var expected = new ForInExpression("k", new ArrayExpression([new ConstantExpression(1L), new ConstantExpression(2L), new ConstantExpression(3L)]), new CallExpression("puts", [new NameExpression("k")]));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleForInCommandWithDoUsingReader()
        {
            Parser parser = new(new StringReader("for k in [1,2,3] do\n puts k\nend"));
            var expected = new ForInExpression("k", new ArrayExpression([new ConstantExpression(1L), new ConstantExpression(2L), new ConstantExpression(3L)]), new CallExpression("puts", [new NameExpression("k")]));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleForInCommandWithDoSingleLine()
        {
            Parser parser = new("for k in [1,2,3] do puts(k) end");
            var expected = new ForInExpression("k", new ArrayExpression([new ConstantExpression(1L), new ConstantExpression(2L), new ConstantExpression(3L)]), new CallExpression("puts", [new NameExpression("k")]));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleForInCommandSingleLine()
        {
            Parser parser = new("for k in [1,2,3]; puts k; end");
            var expected = new ForInExpression("k", new ArrayExpression([new ConstantExpression(1L), new ConstantExpression(2L), new ConstantExpression(3L)]), new CallExpression("puts", [new NameExpression("k")]));
            var result = parser.ParseCommand();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleIndexedExpression()
        {
            Parser parser = new("a[1]");
            var expected = new IndexedExpression(new NameExpression("a"), new ConstantExpression(1L));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseCallWithDo()
        {
            Parser parser = new("k.times do print 'foo' end");
            var expected = new DotExpression(new NameExpression("k"), "times", [new BlockExpression(null, new CallExpression("print", [new ConstantExpression("foo")]))]);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseCallWithDoWithArguments()
        {
            Parser parser = new("1.upto(9) do |x| print x end");
            var expected = new DotExpression(new ConstantExpression(1L), "upto", [new ConstantExpression(9L), new BlockExpression(["x"], new CallExpression("print", [new NameExpression("x")]))]);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseCallWithBlock()
        {
            Parser parser = new("k.times { print 'foo' }");
            var expected = new DotExpression(new NameExpression("k"), "times", [new BlockExpression(null, new CallExpression("print", [new ConstantExpression("foo")]))]);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseNameCallWithDo()
        {
            Parser parser = new("map do print 'foo' end");
            var expected = new CallExpression("map", [new BlockExpression(null, new CallExpression("print", [new ConstantExpression("foo")]))]);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseNameCallWithBlock()
        {
            Parser parser = new("map { print 'foo' }");
            var expected = new CallExpression("map", [new BlockExpression(null, new CallExpression("print", [new ConstantExpression("foo")]))]);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseCallWithBlockWithArguments()
        {
            Parser parser = new("1.upto(9) { |x| print x }");
            var expected = new DotExpression(new ConstantExpression(1L), "upto", [new ConstantExpression(9L), new BlockExpression(["x"], new CallExpression("print", [new NameExpression("x")]))]);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseEmptyModule()
        {
            Parser parser = new("module Module1; a=1; end");
            var expected = new ModuleExpression("Module1", new AssignExpression("a", new ConstantExpression(1L)));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseDoubleColon()
        {
            Parser parser = new("Module1::PI");
            var expected = new DoubleColonExpression(new NameExpression("Module1"), "PI");
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseChainedDot()
        {
            Parser parser = new("Object.new.class");
            var expected = new DotExpression(new DotExpression(new NameExpression("Object"), "new", []), "class", []);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleHash()
        {
            Parser parser = new("{ :one=>1, :two => 2 }");
            var expected = new HashExpression([new ConstantExpression(new Symbol("one")), new ConstantExpression(new Symbol("two"))], [new ConstantExpression(1L), new ConstantExpression(2L)]);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseEmptyHash()
        {
            Parser parser = new("{  }");
            var expected = new HashExpression([], []);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleIndexedAssign()
        {
            Parser parser = new("a[1] = 2");
            var expected = new AssignIndexedExpression(new NameExpression("a"), new ConstantExpression(1L), new ConstantExpression(2L));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseDotIndexedAssign()
        {
            Parser parser = new("a.b[1] = 2");
            var expected = new AssignIndexedExpression(new DotExpression(new NameExpression("a"), "b", []), new ConstantExpression(1L), new ConstantExpression(2L));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseSimpleRangeExpression()
        {
            Parser parser = new("1..10");
            var expected = new RangeExpression(new ConstantExpression(1L), new ConstantExpression(10L));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void ParseRangeExpression()
        {
            Parser parser = new("1+2..10");
            var expected = new RangeExpression(new AddExpression(new ConstantExpression(1L), new ConstantExpression(2L)), new ConstantExpression(10L));
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }

        [TestMethod]
        public void RaiseIfModuleNameIsNotAConstant()
        {
            Parser parser = new("module mymod end");

            try
            {
                parser.ParseExpression();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("class/module name must be a CONSTANT", ex.Message);
            }
        }

        [TestMethod]
        public void RaiseIfClassNameIsNotAConstant()
        {
            Parser parser = new("class myclass\nend");

            try
            {
                parser.ParseExpression();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("class/module name must be a CONSTANT", ex.Message);
            }
        }

        [TestMethod]
        public void ParseSelfExpression()
        {
            Parser parser = new("self");
            var expected = new SelfExpression();
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);

            Assert.IsNull(parser.ParseCommand());
        }
    }
}

