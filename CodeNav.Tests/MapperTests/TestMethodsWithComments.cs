using System;
using System.IO;
using System.Linq;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TestMethodsWithComments
    {
        [Test]
        public void ShouldBeOk()
        {
            var document = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestMethodsWithComments.cs"));

            Assert.IsTrue(document.Any());

            // First item should be a namespace
            Assert.AreEqual(CodeItemKindEnum.Namespace, document.First().Kind);

            // Inner item should be a class
            var innerClass = (document.First() as CodeNamespaceItem).Members.First() as CodeClassItem;

            // Class should have a method
            var methodWithComment = innerClass.Members.First() as CodeFunctionItem;

            Assert.AreEqual("Super important summary", methodWithComment.Tooltip);

            // Class should have a method
            var methodWithoutComment = innerClass.Members[1] as CodeFunctionItem;

            Assert.AreEqual("Public void MethodWithoutComment ()", methodWithoutComment.Tooltip);

            // Class should have a method
            var methodWithMultipleComment = innerClass.Members[2] as CodeFunctionItem;

            Assert.AreEqual("Multiple comment - summary", methodWithMultipleComment.Tooltip);

            // Class should have a method
            var methodWithReorderedComment = innerClass.Members[3] as CodeFunctionItem;

            Assert.AreEqual("Multiple comment - summary", methodWithReorderedComment.Tooltip);
        }
    }
}
