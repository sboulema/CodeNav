using CodeNav.Mappers.JavaScript;
using CodeNav.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeNav.Tests.MapperTests.JavaScript
{
    [TestFixture]
    public class TestFunctions
    {
        List<CodeItem> document;
        CodeClassItem root;

        [OneTimeSetUp]
        public void Init()
        {
            document = SyntaxMapperJS.Map(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\JavaScript\\TestFunction.js"), null);

            Assert.IsTrue(document.Any());
            Assert.AreEqual(CodeItemKindEnum.Namespace, document.First().Kind);
            root = (document.First() as CodeNamespaceItem).Members.First() as CodeClassItem;
        }

        [Test]
        public void TestBasicFunction()
        {
            Assert.AreEqual("firstFunction", root.Members.FirstOrDefault().Name);
            Assert.AreEqual(3, root.Members.FirstOrDefault().EndLine);
            Assert.AreEqual(0, root.Members.FirstOrDefault().Span.Start);
            Assert.AreEqual(51, root.Members.FirstOrDefault().Span.End);
        }

        [Test]
        public void TestFunctionWithParams()
        {
            Assert.AreEqual("secondFunction", root.Members[1].Name);
            Assert.AreEqual("(input)", (root.Members[1] as CodeFunctionItem).Parameters);
        }

        [Test]
        public void TestAssignedFunction()
        {
            Assert.AreEqual("assignedFunction", root.Members[2].Name);
            Assert.AreEqual("(a, b)", (root.Members[2] as CodeFunctionItem).Parameters);
        }

        [Test]
        public void TestFunctionConstructor()
        {
            Assert.AreEqual("myFunction", root.Members[3].Name);
            Assert.AreEqual("()", (root.Members[3] as CodeFunctionItem).Parameters);
        }

        [Test]
        public void TestArrowFunction()
        {
            Assert.AreEqual("x", root.Members[4].Name);
            Assert.AreEqual("(x, y)", (root.Members[4] as CodeFunctionItem).Parameters);
        }

        [Test]
        public void TestOuterInnerFunction()
        {
            Assert.AreEqual("outerFunction", root.Members[5].Name);
            Assert.AreEqual(2, (root.Members[5] as CodeClassItem).Members.Count);
            Assert.AreEqual("innerFunction", (root.Members[5] as CodeClassItem).Members.FirstOrDefault().Name);
            Assert.AreEqual("assignedInnerFunction", (root.Members[5] as CodeClassItem).Members[1].Name);
            Assert.AreEqual("(a, b)", ((root.Members[5] as CodeClassItem).Members[1] as CodeFunctionItem).Parameters);
        }

        [Test]
        public void TestAsyncFunction()
        {
            Assert.AreEqual("asyncFunction", root.Members[6].Name);
            Assert.AreEqual(27, root.Members[6].StartLine);
        }
    }
}
