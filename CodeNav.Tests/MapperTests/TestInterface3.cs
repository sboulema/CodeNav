using System;
using System.IO;
using System.Linq;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TestInterface3
    {
        [Test]
        public void TestBaseImplementedInterfaceShouldBeOk()
        {
            var document = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestInterface3.cs"), null);

            Assert.IsTrue(document.Any());

            // last item should be the implementing class
            var implementingClass = (document.First() as IMembers).Members.Last() as CodeClassItem;

            Assert.AreEqual(CodeItemKindEnum.Class, implementingClass.Kind);
            Assert.AreEqual(1, implementingClass.Members.Count);

            var implementedInterface = implementingClass.Members.First() as CodeImplementedInterfaceItem;

            Assert.AreEqual(CodeItemKindEnum.ImplementedInterface, implementedInterface.Kind);
            Assert.AreEqual(1, implementedInterface.Members.Count);
        }
    }
}
