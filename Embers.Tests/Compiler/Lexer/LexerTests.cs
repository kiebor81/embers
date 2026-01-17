using Embers.Compiler;
using Embers.Exceptions;

namespace Embers.Tests.Compiler
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void GetName()
        {
            Lexer lexer = new("name");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("name", result.Value);
            Assert.AreEqual(TokenType.Name, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetNameWithDigits()
        {
            Lexer lexer = new("name123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("name123", result.Value);
            Assert.AreEqual(TokenType.Name, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetNameWithUnderscore()
        {
            Lexer lexer = new("name_123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("name_123", result.Value);
            Assert.AreEqual(TokenType.Name, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetNameWithInitialUnderscore()
        {
            Lexer lexer = new("_foo");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("_foo", result.Value);
            Assert.AreEqual(TokenType.Name, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void RaiseIfUnexpectedCharacter()
        {
            Lexer lexer = new("\\");

            try
            {
                lexer.NextToken();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("unexpected '\\'", ex.Message);
            }
        }

        [TestMethod]
        public void GetNameWithSpaces()
        {
            Lexer lexer = new("  name   ");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("name", result.Value);
            Assert.AreEqual(TokenType.Name, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void SkipComment()
        {
            Lexer lexer = new("# this is a comment");

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void SkipCommentUpToEndOfLine()
        {
            Lexer lexer = new("# this is a comment\n");

            var token = lexer.NextToken();

            Assert.IsNotNull(token);
            Assert.AreEqual(TokenType.EndOfLine, token.Type);
            Assert.AreEqual("\n", token.Value);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetNamesSkippingCommentUpToEndOfLine()
        {
            Lexer lexer = new("a# this is a comment\nb");

            var token = lexer.NextToken();

            Assert.IsNotNull(token);
            Assert.AreEqual(TokenType.Name, token.Type);
            Assert.AreEqual("a", token.Value);

            token = lexer.NextToken();

            Assert.IsNotNull(token);
            Assert.AreEqual(TokenType.EndOfLine, token.Type);
            Assert.AreEqual("\n", token.Value);

            token = lexer.NextToken();

            Assert.IsNotNull(token);
            Assert.AreEqual(TokenType.Name, token.Type);
            Assert.AreEqual("b", token.Value);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetSymbol()
        {
            Lexer lexer = new(":foo");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("foo", result.Value);
            Assert.AreEqual(TokenType.Symbol, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetSymbolWithDigits()
        {
            Lexer lexer = new(":foo123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("foo123", result.Value);
            Assert.AreEqual(TokenType.Symbol, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetSymbolWithUnderscore()
        {
            Lexer lexer = new(":foo_123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("foo_123", result.Value);
            Assert.AreEqual(TokenType.Symbol, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetSymbolWithClassVarPrefix()
        {
            Lexer lexer = new(":@@value_0");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("@@value_0", result.Value);
            Assert.AreEqual(TokenType.Symbol, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetSymbolWithInitialUnderscore()
        {
            Lexer lexer = new(":_123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("_123", result.Value);
            Assert.AreEqual(TokenType.Symbol, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetSymbolWithSpaces()
        {
            Lexer lexer = new(" :_123 ");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("_123", result.Value);
            Assert.AreEqual(TokenType.Symbol, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void RaiseIfSymbolStartsWithADigit()
        {
            Lexer lexer = new(":123");

            try
            {
                lexer.NextToken();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("unexpected integer", ex.Message);
            }
        }

        [TestMethod]
        public void GetInstanceVarName()
        {
            Lexer lexer = new("@a");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("a", result.Value);
            Assert.AreEqual(TokenType.InstanceVarName, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetInstanceVarNameWithDigits()
        {
            Lexer lexer = new("@a123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("a123", result.Value);
            Assert.AreEqual(TokenType.InstanceVarName, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetInstanceVarNameWithDigitsAndUnderscore()
        {
            Lexer lexer = new("@a_123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("a_123", result.Value);
            Assert.AreEqual(TokenType.InstanceVarName, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetInstanceVarNameWithInitialUnderscore()
        {
            Lexer lexer = new("@_123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("_123", result.Value);
            Assert.AreEqual(TokenType.InstanceVarName, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetClassVarName()
        {
            Lexer lexer = new("@@a");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("a", result.Value);
            Assert.AreEqual(TokenType.ClassVarName, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetClassVarNameWithDigits()
        {
            Lexer lexer = new("@@a123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("a123", result.Value);
            Assert.AreEqual(TokenType.ClassVarName, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetClassVarNameWithDigitsAndUnderscore()
        {
            Lexer lexer = new("@@a_123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("a_123", result.Value);
            Assert.AreEqual(TokenType.ClassVarName, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetClassVarNameWithInitialUnderscore()
        {
            Lexer lexer = new("@@_123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("_123", result.Value);
            Assert.AreEqual(TokenType.ClassVarName, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetGlobalVarName()
        {
            Lexer lexer = new("$a");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("a", result.Value);
            Assert.AreEqual(TokenType.GlobalVarName, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetGlobalVarNameWithDigits()
        {
            Lexer lexer = new("$a123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("a123", result.Value);
            Assert.AreEqual(TokenType.GlobalVarName, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetGlobalVarNameWithDigitsAndUnderscore()
        {
            Lexer lexer = new("$a_123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("a_123", result.Value);
            Assert.AreEqual(TokenType.GlobalVarName, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetGlobalVarNameWithInitialUnderscore()
        {
            Lexer lexer = new("$_123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("_123", result.Value);
            Assert.AreEqual(TokenType.GlobalVarName, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void RaiseWhenInvalidInstanceVarName()
        {
            Lexer lexer = new("@");

            try
            {
                lexer.NextToken();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("invalid instance variable name", ex.Message);
            }
        }

        [TestMethod]
        public void RaiseWhenInvalidGlobalVarName()
        {
            Lexer lexer = new("$");

            try
            {
                lexer.NextToken();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("invalid global variable name", ex.Message);
            }
        }

        [TestMethod]
        public void RaiseWhenInvalidClassVarName()
        {
            Lexer lexer = new("@@");

            try
            {
                lexer.NextToken();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("invalid class variable name", ex.Message);
            }
        }

        [TestMethod]
        public void RaiseWhenInstanceVarNameStartsWithADigit()
        {
            Lexer lexer = new("@123");

            try
            {
                lexer.NextToken();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("invalid instance variable name", ex.Message);
            }
        }

        [TestMethod]
        public void RaiseWhenClassVarNameStartsWithADigit()
        {
            Lexer lexer = new("@@123");

            try
            {
                lexer.NextToken();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("invalid class variable name", ex.Message);
            }
        }

        [TestMethod]
        public void RaiseWhenGlobalVarNameStartsWithADigit()
        {
            Lexer lexer = new("$123");

            try
            {
                lexer.NextToken();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SyntaxError));
                Assert.AreEqual("invalid global variable name", ex.Message);
            }
        }

        [TestMethod]
        public void GetInteger()
        {
            Lexer lexer = new("123");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("123", result.Value);
            Assert.AreEqual(TokenType.Integer, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetIntegerWithSpaces()
        {
            Lexer lexer = new("  123   ");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("123", result.Value);
            Assert.AreEqual(TokenType.Integer, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetIntegerWithDotName()
        {
            Lexer lexer = new("123.foo");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("123", result.Value);
            Assert.AreEqual(TokenType.Integer, result.Type);

            result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual(".", result.Value);
            Assert.AreEqual(TokenType.Separator, result.Type);

            result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("foo", result.Value);
            Assert.AreEqual(TokenType.Name, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetRealNumber()
        {
            Lexer lexer = new("123.45");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("123.45", result.Value);
            Assert.AreEqual(TokenType.Real, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetRealNumberWithSpaces()
        {
            Lexer lexer = new("  123.45   ");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("123.45", result.Value);
            Assert.AreEqual(TokenType.Real, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetRealNumberWithSpacesUsingReader()
        {
            Lexer lexer = new(new StringReader("  123.45   "));
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("123.45", result.Value);
            Assert.AreEqual(TokenType.Real, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetSingleQuoteString()
        {
            Lexer lexer = new("'foo'");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("foo", result.Value);
            Assert.AreEqual(TokenType.String, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetSingleQuoteStringWithCommentChar()
        {
            Lexer lexer = new("'#foo'");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("#foo", result.Value);
            Assert.AreEqual(TokenType.String, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        [ExpectedException(typeof(SyntaxError))]
        public void RaiseIfSingleQuoteStringIsNotClosed()
        {
            Lexer lexer = new("'foo");
            lexer.NextToken();
        }

        [TestMethod]
        public void GetDoubleQuoteString()
        {
            Lexer lexer = new("\"foo\"");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("foo", result.Value);
            Assert.AreEqual(TokenType.String, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetStringWithEscapedChars()
        {
            Lexer lexer = new("\"foo\\t\\n\\r\"");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("foo\t\n\r", result.Value);
            Assert.AreEqual(TokenType.String, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        [ExpectedException(typeof(SyntaxError))]
        public void RaiseIfDoubleQuoteStringIsNotClosed()
        {
            Lexer lexer = new("\"foo");
            lexer.NextToken();
        }

        [TestMethod]
        public void GetAssignOperator()
        {
            Lexer lexer = new("=");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("=", result.Value);
            Assert.AreEqual(TokenType.Operator, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetArrowOperator()
        {
            Lexer lexer = new("=>");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("=>", result.Value);
            Assert.AreEqual(TokenType.Operator, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetRangeOperator()
        {
            Lexer lexer = new("..");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("..", result.Value);
            Assert.AreEqual(TokenType.Operator, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetComparisonOperators()
        {
            Lexer lexer = new("== != < > <= >=");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("==", result.Value);
            Assert.AreEqual(TokenType.Operator, result.Type);

            result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("!=", result.Value);
            Assert.AreEqual(TokenType.Operator, result.Type);

            result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("<", result.Value);
            Assert.AreEqual(TokenType.Operator, result.Type);

            result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual(">", result.Value);
            Assert.AreEqual(TokenType.Operator, result.Type);

            result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("<=", result.Value);
            Assert.AreEqual(TokenType.Operator, result.Type);

            result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual(">=", result.Value);
            Assert.AreEqual(TokenType.Operator, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetSemicolonAsSeparator()
        {
            Lexer lexer = new(";");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual(";", result.Value);
            Assert.AreEqual(TokenType.Separator, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetCommaAsSeparator()
        {
            Lexer lexer = new(",");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual(",", result.Value);
            Assert.AreEqual(TokenType.Separator, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetVerticalBarAsSeparator()
        {
            Lexer lexer = new("|");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("|", result.Value);
            Assert.AreEqual(TokenType.Separator, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetDoubleColonAsSeparator()
        {
            Lexer lexer = new("::");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("::", result.Value);
            Assert.AreEqual(TokenType.Separator, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetParenthesesAsSeparators()
        {
            Lexer lexer = new("()");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("(", result.Value);
            Assert.AreEqual(TokenType.Separator, result.Type);

            result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual(")", result.Value);
            Assert.AreEqual(TokenType.Separator, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetBracketsAsSeparators()
        {
            Lexer lexer = new("[]");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("[", result.Value);
            Assert.AreEqual(TokenType.Separator, result.Type);

            result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("]", result.Value);
            Assert.AreEqual(TokenType.Separator, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetBracesAsSeparators()
        {
            Lexer lexer = new("{}");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("{", result.Value);
            Assert.AreEqual(TokenType.Separator, result.Type);

            result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("}", result.Value);
            Assert.AreEqual(TokenType.Separator, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetPlusAsOperator()
        {
            Lexer lexer = new("+");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual("+", result.Value);
            Assert.AreEqual(TokenType.Operator, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetEndOfLineWithNewLine()
        {
            Lexer lexer = new("\n");
            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenType.EndOfLine, result.Type);

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetFourArithmeticOperators()
        {
            Lexer lexer = new("+ - * /");

            for (int k = 0; k < 4; k++)
            {
                var result = lexer.NextToken();
                Assert.IsNotNull(result);
                Assert.AreEqual(TokenType.Operator, result.Type);
                Assert.IsNotNull(result.Value);
                Assert.AreEqual(1, result.Value.Length);
                Assert.AreEqual("+-*/"[k], result.Value[0]);
            }

            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        public void GetSimpleAdd()
        {
            Lexer lexer = new("1+2");

            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenType.Integer, result.Type);
            Assert.AreEqual("1", result.Value);

            result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenType.Operator, result.Type);
            Assert.AreEqual("+", result.Value);

            result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenType.Integer, result.Type);
            Assert.AreEqual("2", result.Value);
        }

        [TestMethod]
        public void GetSimpleAddNames()
        {
            Lexer lexer = new("one+two");

            var result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenType.Name, result.Type);
            Assert.AreEqual("one", result.Value);

            result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenType.Operator, result.Type);
            Assert.AreEqual("+", result.Value);

            result = lexer.NextToken();

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenType.Name, result.Type);
            Assert.AreEqual("two", result.Value);
        }
    }
}
