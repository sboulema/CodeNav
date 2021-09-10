using System;
using System.IO;
using System.Linq;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TestInterface4
    {
        [Test]
        public void TestClassImplementedInterfaceAndBaseImplementedInterfaceShouldBeOk()
        {
            var document = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestInterface4.cs"), null);

            Assert.IsTrue(document.Any());

            // last item should be the implementing class
            var implementingClass = (document.First() as IMembers).Members.Last() as CodeClassItem;

            Assert.AreEqual(CodeItemKindEnum.Class, implementingClass.Kind);
            Assert.AreEqual(1, implementingClass.Members.Count);

            var method = implementingClass.Members.First() as CodeFunctionItem;

            Assert.AreEqual(CodeItemKindEnum.Method, method.Kind);
            Assert.AreEqual("ClassAMethod", method.Name);
        }
    }
}
