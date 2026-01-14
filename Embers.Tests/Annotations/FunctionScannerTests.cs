using Embers.Annotations;
using Embers.Language;
using Embers.StdLib;

namespace Embers.Tests.Annotations
{
    [TestClass]
    public class FunctionScannerTests
    {
        [TestMethod]
        public void ScanFunctionDocumentation_ReturnsNonEmptyDictionary()
        {
            var documentation = FunctionScanner.ScanFunctionDocumentation();
            
            Assert.IsNotNull(documentation);
            Assert.IsTrue(documentation.Count > 0, "Should find at least one IFunction implementation");
        }

        [TestMethod]
        public void ScanFunctionDocumentation_FindsPutsFunction()
        {
            var documentation = FunctionScanner.ScanFunctionDocumentation();
            
            Assert.IsTrue(documentation.ContainsKey("puts, print"), 
                "Should find puts, print in scanned functions");
        }

        [TestMethod]
        public void ScanFunctionDocumentation_PutsFunctionHasComments()
        {
            var documentation = FunctionScanner.ScanFunctionDocumentation();
            var (Comments, Arguments, Returns) = documentation["puts, print"];
            
            Assert.IsFalse(string.IsNullOrEmpty(Comments), 
                "PutsFunction should have comments attribute");
            Assert.IsTrue(Comments.Contains("Prints"), 
                "Comments should contain 'Prints'");
        }

        [TestMethod]
        public void ScanFunctionDocumentation_PutsFunctionHasArguments()
        {
            var documentation = FunctionScanner.ScanFunctionDocumentation();
            var (Comments, Arguments, Returns) = documentation["puts, print"];
            
            Assert.IsFalse(string.IsNullOrEmpty(Arguments), 
                "PutsFunction should have arguments attribute");
            Assert.IsTrue(Arguments.Contains("input"), 
                "Arguments should contain 'input' parameter");
        }

        [TestMethod]
        public void ScanFunctionDocumentation_PutsFunctionHasReturnType()
        {
            var documentation = FunctionScanner.ScanFunctionDocumentation();
            var (Comments, Arguments, Returns) = documentation["puts, print"];
            
            Assert.IsFalse(string.IsNullOrEmpty(Returns), 
                "PutsFunction should have return type attribute");
            Assert.AreEqual("Void", Returns, 
                "PutsFunction should return Void");
        }

        [TestMethod]
        public void ScanFunctionDocumentation_AllEntriesHaveCorrectTupleStructure()
        {
            var documentation = FunctionScanner.ScanFunctionDocumentation();
            
            foreach (var entry in documentation)
            {
                Assert.IsNotNull(entry.Value.Comments, 
                    $"{entry.Key} Comments should not be null");
                Assert.IsNotNull(entry.Value.Arguments, 
                    $"{entry.Key} Arguments should not be null");
                Assert.IsNotNull(entry.Value.Returns, 
                    $"{entry.Key} Returns should not be null");
            }
        }

        [TestMethod]
        public void ScanFunctionDocumentation_HandlesEmptyAttributesGracefully()
        {
            var documentation = FunctionScanner.ScanFunctionDocumentation();
            
            // Find a function with potentially missing attributes
            // (if any exist without all three attributes, they should still be in the dictionary)
            var hasEmptyAttribute = documentation.Values.Any(t => 
                t.Comments == string.Empty || 
                t.Arguments == string.Empty || 
                t.Returns == string.Empty);
            
            // This test just verifies that functions without attributes don't cause errors
            Assert.IsTrue(documentation.Count > 0, 
                "Should handle functions with missing attributes gracefully");
        }

        [TestMethod]
        public void ScanFunctionDocumentation_IncludesStdLibFunctions()
        {
            var documentation = FunctionScanner.ScanFunctionDocumentation();
            
            // Should include various StdLib functions by their registered names
            Assert.IsTrue(documentation.Count > 0, 
                "Should include StdLib function implementations");
            
            // Verify at least some expected StdLib functions are present
            var hasStdLibFunctions = documentation.Keys.Any(k => 
                k.Contains("puts") || k.Contains("upcase") || k.Contains("downcase"));
            
            Assert.IsTrue(hasStdLibFunctions, 
                "Should include expected StdLib functions like puts, upcase, downcase");
        }

        [TestMethod]
        public void ScanFunctionDocumentation_SkipsClassesWithScannerIgnoreAttribute()
        {
            var documentation = FunctionScanner.ScanFunctionDocumentation();
            
            // IgnoredTestFunction should not be in the scanned results
            Assert.IsFalse(documentation.ContainsKey("ignored_test"), 
                "Functions with ScannerIgnoreAttribute should be skipped");
            Assert.IsFalse(documentation.ContainsKey("IgnoredTestFunction"), 
                "Functions with ScannerIgnoreAttribute should not appear by class name");
        }
    }

    // Test class with ScannerIgnoreAttribute - should be skipped during scanning
    [ScannerIgnore]
    [StdLib("ignored_test")]
    public class IgnoredTestFunction : StdFunction
    {
        [Comments("This function should be ignored.")]
        [Arguments(ParamNames = new[] { "value" }, ParamTypes = new[] { typeof(object) })]
        [Returns(ReturnType = typeof(string))]
        public override object Apply(DynamicObject self, Context context, IList<object> values) => "ignored";
    }
}
