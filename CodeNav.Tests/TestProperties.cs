using System.Linq;
using CodeNav.Mappers;
using CodeNav.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeNav.Tests
{
    [TestClass]
    public class TestProperties
    {
        [TestMethod]
        public void ShouldBeOk()
        {
            var document = SyntaxMapper.MapDocument("..\\..\\Files\\TestProperties.cs");

            Assert.IsTrue(document.Any());

            // First item should be a namespace
            Assert.AreEqual(CodeItemKindEnum.Namespace, document.First().Kind);

            // Namespace item should have 1 member
            Assert.AreEqual(1, (document.First() as IMembers).Members.Count);

            // Inner item should be a class
            var innerClass = (document.First() as IMembers).Members.First() as CodeClassItem;

            // Inheriting class should have properties
            var propertyGetSet = innerClass.Members.First() as CodeFunctionItem;
            Assert.AreEqual("PropertyGetSet {get,set}", propertyGetSet.Parameters);

            var propertyGet = innerClass.Members[1] as CodeFunctionItem;
            Assert.AreEqual("PropertyGet {get}", propertyGet.Parameters);

            var propertySet = innerClass.Members[2] as CodeFunctionItem;
            Assert.AreEqual("PropertySet {set}", propertySet.Parameters);

            var property = innerClass.Members.Last() as CodeFunctionItem;
            Assert.AreEqual("Property", property.Parameters);
        }
    }
}
