using System;
using System.IO;
using System.Linq;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests
{
    [TestFixture]
    public class TestNestedNamespaces
    {
        [Test]
        public void ShouldHaveCorrectStructure()
        {
            var document = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestNestedNamespaces.cs"));

            Assert.IsTrue(document.Any());

            // First item should be a namespace
            Assert.AreEqual(CodeItemKindEnum.Namespace, document.First().Kind);

            // Namespace item should have members
            Assert.IsTrue((document.First() as IMembers).Members.Any());

            // Inner item should also be a namespace
            var innerNamespace = (document.First() as IMembers).Members.First() as CodeNamespaceItem;
            Assert.AreEqual(CodeItemKindEnum.Namespace, innerNamespace.Kind);

            // That inner namespace should have members
            Assert.IsTrue(innerNamespace.Members.Any());

            // That member should be a class
            var innerClass = (innerNamespace as IMembers).Members.First() as CodeClassItem;
            Assert.AreEqual(CodeItemKindEnum.Class, innerClass.Kind);
            Assert.AreEqual("ClassInNestedNamespace", innerClass.Name);
        }
    }
}
