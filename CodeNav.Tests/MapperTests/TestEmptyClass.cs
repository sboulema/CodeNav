using System;
using System.IO;
using System.Linq;
using System.Windows;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TestEmptyClass
    {
        [Test]
        public void ShouldBeVisible()
        {
            var document = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestEmptyClass.cs"));

            Assert.IsTrue(document.Any());

            // First item should be a namespace
            Assert.AreEqual(CodeItemKindEnum.Namespace, document.First().Kind);

            // Namespace item should have members
            Assert.IsTrue((document.First() as IMembers).Members.Any());

            // Inner item should be a class
            var innerClass = (document.First() as IMembers).Members.First() as CodeClassItem;
            Assert.AreEqual(CodeItemKindEnum.Class, innerClass.Kind);
            Assert.AreEqual("CodeNavTestEmptyClass", innerClass.Name);

            // Class should be visible
            Assert.AreEqual(Visibility.Visible, innerClass.IsVisible);

            // Since it does not have members, it should not show the expander symbol
            Assert.AreEqual(Visibility.Collapsed, innerClass.HasMembersVisibility);
        }

        [Test]
        public void ShouldHaveCorrectDefaultAccess()
        {
            var document = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestEmptyClass.cs"));

            Assert.IsTrue(document.Any());

            // First item should be a namespace
            Assert.AreEqual(CodeItemKindEnum.Namespace, document.First().Kind);

            // Namespace item should have members
            Assert.IsTrue((document.First() as IMembers).Members.Any());

            // Inner item should be a class
            var innerClass = (document.First() as IMembers).Members.First() as CodeClassItem;
            
            Assert.AreEqual(CodeItemAccessEnum.Internal, innerClass.Access);
            Assert.IsTrue(innerClass.IconPath.Contains("Friend"));
        }
    }
}
