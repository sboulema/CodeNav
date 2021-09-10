using System;
using System.IO;
using System.Linq;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TestEnums
    {
        [Test]
        public void EnumsShouldBeOkVB()
        {
            var document = SyntaxMapper.MapDocumentVB(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\VisualBasic\\TestModules.vb"), null);

            Assert.IsTrue(document.Any());

            var innerEnum = (document.First() as IMembers).Members.First() as CodeClassItem;

            // First inner item should be an enum
            Assert.AreEqual(CodeItemKindEnum.Enum, innerEnum.Kind);      

            // Enum should have 5 members
            Assert.AreEqual(5, innerEnum.Members.Count());
        }
    }
}
