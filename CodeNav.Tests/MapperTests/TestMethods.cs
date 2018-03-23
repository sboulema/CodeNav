using System;
using System.IO;
using System.Linq;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TestMethods
    {
        [Test]
        public void ConstructorShouldBeOkVB()
        {
            var document = SyntaxMapper.MapDocumentVB(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\VisualBasic\\TestMethods.vb"));

            Assert.IsTrue(document.Any());

            // First item should be a class
            Assert.AreEqual(CodeItemKindEnum.Class, document.First().Kind);

            // Inner item should be a class
            var innerClass = document.First() as CodeClassItem;

            // Class should have a constructor
            var constructor = innerClass.Members.First() as CodeFunctionItem;
            Assert.AreEqual(CodeItemAccessEnum.Public, constructor.Access);
            Assert.AreEqual(CodeItemKindEnum.Constructor, constructor.Kind);
            Assert.AreEqual("New", constructor.Name);
        }

        [Test]
        public void FunctionShouldBeOkVB()
        {
            var document = SyntaxMapper.MapDocumentVB(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\VisualBasic\\TestMethods.vb"));

            Assert.IsTrue(document.Any());

            // First item should be a class
            Assert.AreEqual(CodeItemKindEnum.Class, document.First().Kind);

            // Inner item should be a class
            var innerClass = document.First() as CodeClassItem;

            // Class should have a function
            var method = innerClass.Members[1] as CodeFunctionItem;
            Assert.AreEqual(CodeItemAccessEnum.Private, method.Access);
            Assert.AreEqual(CodeItemKindEnum.Method, method.Kind);
            Assert.AreEqual("Area", method.Name);
            Assert.AreEqual("Double", method.Type);
        }

        [Test]
        public void SubShouldBeOkVB()
        {
            var document = SyntaxMapper.MapDocumentVB(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\VisualBasic\\TestMethods.vb"));

            Assert.IsTrue(document.Any());

            // First item should be a class
            Assert.AreEqual(CodeItemKindEnum.Class, document.First().Kind);

            // Inner item should be a class
            var innerClass = document.First() as CodeClassItem;

            // Class should have a sub
            var method = innerClass.Members.Last() as CodeFunctionItem;
            Assert.AreEqual(CodeItemAccessEnum.Internal, method.Access);
            Assert.AreEqual(CodeItemKindEnum.Method, method.Kind);
            Assert.AreEqual("SubInsteadOfFunction", method.Name);
            Assert.AreEqual("", method.Type);
        }
    }
}
