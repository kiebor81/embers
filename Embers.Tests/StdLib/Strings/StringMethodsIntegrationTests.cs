namespace Embers.Tests.StdLib.Strings
{
    [TestClass]
    public class StringMethodsIntegrationTests
    {
        [TestMethod]
        public void String_UpcaseMethod_WorksViaAutomaticDiscovery()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello'.upcase");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("HELLO", result);
        }

        [TestMethod]
        public void String_DowncaseMethod_WorksViaAutomaticDiscovery()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'HELLO'.downcase");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("hello", result);
        }

        [TestMethod]
        public void String_CapitalizeMethod_WorksViaAutomaticDiscovery()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello world'.capitalize");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("Hello world", result);
        }

        [TestMethod]
        public void String_ReverseMethod_WorksViaAutomaticDiscovery()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello'.reverse");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("olleh", result);
        }

        [TestMethod]
        public void String_LengthMethod_WorksViaAutomaticDiscovery()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello'.length");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void String_LengthAlias_LenWorks()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello'.len");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void String_EmptyMethod_ReturnsTrueForEmpty()
        {
            Machine machine = new();
            var result = machine.ExecuteText("''.empty?");

            Assert.IsNotNull(result);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void String_EmptyMethod_ReturnsFalseForNonEmpty()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'a'.empty?");

            Assert.IsNotNull(result);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void String_CasecmpMethod_IgnoresCase()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'abc'.casecmp('ABC')");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void String_MatchMethod_ReturnsMatch()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'abc123'.match('\\\\d+')");

            Assert.IsNotNull(result);
            Assert.AreEqual("123", result);
        }

        [TestMethod]
        public void String_MatchMethod_ReturnsNilWhenNoMatch()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'abc'.match('\\\\d+')");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void String_StripMethod_RemovesWhitespace()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'  hello  '.strip");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("hello", result);
        }

        [TestMethod]
        public void String_SplitMethod_SplitsString()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello world'.split");
            
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IEnumerable<string>));
            
            var list = ((IEnumerable<string>)result).ToList();
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("hello", list[0]);
            Assert.AreEqual("world", list[1]);
        }

        [TestMethod]
        public void String_SwapcaseMethod_SwapsCase()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'Hello World'.swapcase");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("hELLO wORLD", result);
        }

        [TestMethod]
        public void String_ChompMethod_RemovesTrailingNewline()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello\\n'.chomp");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("hello", result);
        }

        [TestMethod]
        public void String_IndexMethod_FindsSubstring()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello world'.index('world')");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public void String_IncludeMethod_ChecksForSubstring()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello world'.include?('world')");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void String_StartWithMethod_ChecksPrefix()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello world'.start_with?('hello')");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void String_EndWithMethod_ChecksSuffix()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello world'.end_with?('world')");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void String_CharsMethod_ReturnsCharArray()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello'.chars");
            
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IEnumerable<string>));
            
            var list = ((IEnumerable<string>)result).ToList();
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual("h", list[0]);
            Assert.AreEqual("o", list[4]);
        }

        [TestMethod]
        public void String_LinesMethod_SplitsByNewlines()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello\\nworld'.lines");
            
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IEnumerable<string>));
            
            var list = ((IEnumerable<string>)result).ToList();
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void String_ChainedMethods_Work()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'  HELLO  '.strip.downcase");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("hello", result);
        }

        [TestMethod]
        public void String_MethodOnVariable_Works()
        {
            Machine machine = new();
            machine.ExecuteText("s = 'hello world'");
            var result = machine.ExecuteText("s.upcase");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("HELLO WORLD", result);
        }

        [TestMethod]
        public void String_MethodWithMultipleArguments_Works()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello world hello'.gsub('hello', 'hi')");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("hi world hi", result);
        }

        [TestMethod]
        public void String_DeleteMethod_RemovesCharacters()
        {
            Machine machine = new();
            var result = machine.ExecuteText("'hello'.delete('l')");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("heo", result);
        }
    }
}
