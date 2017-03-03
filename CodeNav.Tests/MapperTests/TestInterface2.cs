using System;
using System.IO;
using System.Linq;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TestInterface2
    {
        [Test]
        public void TestNestedInterfaceShouldBeOk()
        {
            var document = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestInterface2.cs"));

            Assert.IsTrue(document.Any());

            // First item should be a namespace
            Assert.AreEqual(CodeItemKindEnum.Namespace, document.First().Kind);

            // Namespace item should have 3 members
            Assert.AreEqual(3, (document.First() as IMembers).Members.Count);

            // Second item should be the implementing class
            var implementingClass = (document.First() as IMembers).Members.Last() as CodeClassItem;

            Assert.AreEqual(CodeItemKindEnum.Class, implementingClass.Kind);
            Assert.AreEqual(2, implementingClass.Members.Count);

            var implementedInterface1 = implementingClass.Members.First() as CodeImplementedInterfaceItem;
            var implementedInterface2 = implementingClass.Members.Last() as CodeImplementedInterfaceItem;

            Assert.AreEqual(CodeItemKindEnum.ImplementedInterface, implementedInterface1.Kind);
            Assert.AreEqual(1, implementedInterface1.Members.Count);

            Assert.AreEqual(CodeItemKindEnum.ImplementedInterface, implementedInterface2.Kind);
            Assert.AreEqual(1, implementedInterface1.Members.Count);
        }
    }
}
