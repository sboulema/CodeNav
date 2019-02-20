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
    public class TestVariables
    {
        List<CodeItem> document;
        CodeClassItem root;

        [OneTimeSetUp]
        public void Init()
        {
            document = SyntaxMapperJS.Map(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\JavaScript\\TestVariable.js"), null);

            Assert.IsTrue(document.Any());
            Assert.AreEqual(CodeItemKindEnum.Namespace, document.First().Kind);
            root = (document.First() as CodeNamespaceItem).Members.First() as CodeClassItem;
        }

        [Test]
        public void TestBasicVariable()
        {
            Assert.AreEqual("firstVariable", root.Members.FirstOrDefault().Name);
        }

        [Test]
        public void TestAssignedVariable()
        {
            Assert.AreEqual("assignedVariable", root.Members[1].Name);
            Assert.AreEqual(2, root.Members[1].StartLine);
        }
    }
}
