using System;
using System.IO;
using System.Linq;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TestInterface
    {
        [Test]
        public void TestInterfaceShouldBeOk()
        {
            var document = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestInterface.cs"));

            Assert.IsTrue(document.Any());

            // First item should be a namespace
            Assert.AreEqual(CodeItemKindEnum.Namespace, document.First().Kind);

            // Namespace item should have 2 members
            Assert.AreEqual(2, (document.First() as IMembers).Members.Count);

            // First item should be an interface
            var innerInterface = (document.First() as IMembers).Members.First() as CodeInterfaceItem;
            Assert.AreEqual(3, innerInterface.Members.Count);

            // Second item should be the implementing class
            var implementingClass = (document.First() as IMembers).Members.Last() as CodeClassItem;

            Assert.AreEqual(CodeItemKindEnum.Class, implementingClass.Kind);
            Assert.AreEqual(3, implementingClass.Members.Count);

            var implementedInterface = implementingClass.Members.Last() as CodeInterfaceItem;

            Assert.AreEqual(CodeItemKindEnum.Interface, implementedInterface.Kind);
            Assert.AreEqual(3, implementedInterface.Members.Count);

            // Items shoud be properly replaced
            Assert.AreEqual(12, implementedInterface.Members[0].StartLine);
            Assert.AreEqual(17, implementedInterface.Members[1].StartLine);
            Assert.AreEqual(34, implementedInterface.Members[2].StartLine);
        }
    }
}
