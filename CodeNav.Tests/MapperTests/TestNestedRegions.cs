using System;
using System.IO;
using System.Linq;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TestNestedRegions
    {
        [Test]
        public void NestedRegionsShouldWork()
        {
            var document = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestNestedRegions.cs"));

            Assert.IsTrue(document.Any());

            // First item should be a namespace
            Assert.AreEqual(CodeItemKindEnum.Namespace, document.First().Kind);

            // Namespace item should have members
            Assert.IsTrue((document.First() as IMembers).Members.Any());

            // Namespace should have 1 member
            Assert.AreEqual(1, (document.First() as IMembers).Members.Count);

            // Inner item should be a class
            var innerClass = (document.First() as IMembers).Members.First() as CodeClassItem;
            Assert.AreEqual(CodeItemKindEnum.Class, innerClass.Kind);

            // That inner class should have members
            Assert.IsTrue(innerClass.Members.Any());

            // That member should be a region
            var parentRegion = (innerClass as IMembers).Members.First() as CodeRegionItem;
            Assert.AreEqual(CodeItemKindEnum.Region, parentRegion.Kind);
            Assert.AreEqual("#ParentRegion", parentRegion.Name);

            // That parent region should have members
            Assert.IsTrue(parentRegion.Members.Any());

            // That member should be a region
            var innerRegion = (parentRegion as IMembers).Members.First() as CodeRegionItem;
            Assert.AreEqual(CodeItemKindEnum.Region, innerRegion.Kind);
            Assert.AreEqual("#ChildRegion", innerRegion.Name);
        }
    }
}
