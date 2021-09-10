using System;
using System.IO;
using System.Linq;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TestClasses
    {
        [Test]
        public void ModulesShouldBeOkVB()
        {
            var document = SyntaxMapper.MapDocumentVB(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\VisualBasic\\TestModules.vb"), null);

            Assert.IsTrue(document.Any());

            // First item should be a class
            Assert.AreEqual(CodeItemKindEnum.Class, document.First().Kind);

            // Inner item should be a class
            var innerClass = document.First() as CodeClassItem;

            Assert.True(innerClass.Members.Any());
        }

        [Test]
        public void ClassInheritanceShouldBeOkVB()
        {
            var document = SyntaxMapper.MapDocumentVB(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\VisualBasic\\TestClasses.vb"), null);

            Assert.IsTrue(document.Any());

            // First item should be a base class
            Assert.AreEqual(CodeItemKindEnum.Class, document.First().Kind);
            var innerClass = document.First() as CodeClassItem;

            // Second item should be an inheriting class
            Assert.AreEqual(CodeItemKindEnum.Class, document.Last().Kind);
            var inheritingClass = document.Last() as CodeClassItem;

            Assert.AreEqual(" : Class1", inheritingClass.Parameters);
        }
    }
}
