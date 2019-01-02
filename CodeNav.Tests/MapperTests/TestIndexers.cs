using System;
using System.IO;
using System.Linq;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TestIndexers
    {
        [Test]
        public void ShouldBeOk()
        {
            var document = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestIndexers.cs"));

            Assert.IsTrue(document.Any());

            // First item should be a namespace
            Assert.AreEqual(CodeItemKindEnum.Namespace, document.First().Kind);

            // Inner item should be a class
            var innerClass = (document.First() as CodeNamespaceItem).Members.First() as CodeClassItem;

            // Class should have an indexer
            var indexer = innerClass.Members.First() as CodeFunctionItem;

            Assert.AreEqual(CodeItemKindEnum.Indexer, indexer.Kind);
        }
    }
}
