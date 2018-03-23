using System;
using System.IO;
using System.Linq;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TestFields
    {
        [Test]
        public void ShouldBeOkVB()
        {
            var document = SyntaxMapper.MapDocumentVB(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\VisualBasic\\TestFields.vb"));

            Assert.IsTrue(document.Any());

            // First item should be a class
            Assert.AreEqual(CodeItemKindEnum.Class, document.First().Kind);

            // Inner item should be a class
            var innerClass = document.First() as CodeClassItem;

            // Class should have properties
            var publicConst = innerClass.Members.First() as CodeItem;
            Assert.AreEqual(CodeItemAccessEnum.Public, publicConst.Access);
            Assert.AreEqual(CodeItemKindEnum.Constant, publicConst.Kind);

            var protectedVersion = innerClass.Members[1] as CodeItem;
            Assert.AreEqual(CodeItemAccessEnum.Protected, protectedVersion.Access);
            Assert.AreEqual(CodeItemKindEnum.Variable, protectedVersion.Kind);

            var publicField = innerClass.Members[2] as CodeItem;
            Assert.AreEqual(CodeItemAccessEnum.Public, publicField.Access);
            Assert.AreEqual(CodeItemKindEnum.Variable, publicField.Kind);

            var privateSecret = innerClass.Members[3] as CodeItem;
            Assert.AreEqual(CodeItemAccessEnum.Private, privateSecret.Access);
            Assert.AreEqual(CodeItemKindEnum.Variable, privateSecret.Kind);

            var local = innerClass.Members.Last() as CodeItem;
            Assert.AreEqual(CodeItemAccessEnum.Private, local.Access);
            Assert.AreEqual(CodeItemKindEnum.Variable, local.Kind);
        }
    }
}
