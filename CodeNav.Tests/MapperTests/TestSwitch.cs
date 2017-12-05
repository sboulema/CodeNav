using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TestSwitch
    {
        [Test]
        public void TestSwitchShouldBeOk()
        {
            var document = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestSwitch.cs"));

            Assert.IsTrue(document.Any());

            // First item should be a namespace
            Assert.AreEqual(CodeItemKindEnum.Namespace, document.First().Kind);

            // Namespace item should have 1 member
            Assert.AreEqual(1, (document.First() as IMembers).Members.Count);

            // Inner item should be a class
            var innerClass = (document.First() as IMembers).Members.First() as CodeClassItem;
            Assert.AreEqual(CodeItemKindEnum.Class, innerClass.Kind);

            // Class should have 3 methods
            Assert.AreEqual(3, innerClass.Members.Count);

            // First method should have a switch inside
            var method = innerClass.Members.First();
            var switchStatement = (method as IMembers).Members.First();
            Assert.AreEqual(CodeItemKindEnum.Switch, switchStatement.Kind);

            // Second method should have a switch inside
            var secondMethod = innerClass.Members[1];
            var secondSwitchStatement = (secondMethod as IMembers).Members.First();
            Assert.AreEqual(CodeItemKindEnum.Switch, secondSwitchStatement.Kind);

            // last method should have a switch inside
            var lastMethod = innerClass.Members.Last();
            var lastSwitchStatement = (lastMethod as IMembers).Members.First();
            Assert.AreEqual(CodeItemKindEnum.Switch, lastSwitchStatement.Kind);
        }
    }
}
