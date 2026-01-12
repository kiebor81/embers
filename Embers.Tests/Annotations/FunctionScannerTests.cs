using Embers.Annotations;
using Embers.Functions;
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
            
            Assert.IsTrue(documentation.ContainsKey("PutsFunction"), 
                "Should find PutsFunction in scanned functions");
        }

        [TestMethod]
        public void ScanFunctionDocumentation_PutsFunctionHasComments()
        {
            var documentation = FunctionScanner.ScanFunctionDocumentation();
            var putsDoc = documentation["PutsFunction"];
            
            Assert.IsFalse(string.IsNullOrEmpty(putsDoc.Comments), 
                "PutsFunction should have comments attribute");
            Assert.IsTrue(putsDoc.Comments.Contains("Prints"), 
                "Comments should contain 'Prints'");
        }

        [TestMethod]
        public void ScanFunctionDocumentation_PutsFunctionHasArguments()
        {
            var documentation = FunctionScanner.ScanFunctionDocumentation();
            var putsDoc = documentation["PutsFunction"];
            
            Assert.IsFalse(string.IsNullOrEmpty(putsDoc.Arguments), 
                "PutsFunction should have arguments attribute");
            Assert.IsTrue(putsDoc.Arguments.Contains("input"), 
                "Arguments should contain 'input' parameter");
        }

        [TestMethod]
        public void ScanFunctionDocumentation_PutsFunctionHasReturnType()
        {
            var documentation = FunctionScanner.ScanFunctionDocumentation();
            var putsDoc = documentation["PutsFunction"];
            
            Assert.IsFalse(string.IsNullOrEmpty(putsDoc.Returns), 
                "PutsFunction should have return type attribute");
            Assert.AreEqual("Void", putsDoc.Returns, 
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
            
            // Should include various StdLib functions
            var stdLibFunctionNames = documentation.Keys
                .Where(k => k.EndsWith("Function"))
                .ToList();
            
            Assert.IsTrue(stdLibFunctionNames.Count > 0, 
                "Should include StdLib function implementations");
        }
    }
}
