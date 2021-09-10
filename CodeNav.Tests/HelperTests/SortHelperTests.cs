using CodeNav.Helpers;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace CodeNav.Tests.HelperTests
{
    [TestFixture]
    public class SortHelperTests
    {
        [TestCase(SortOrderEnum.SortByFile, new string[] { "C", "B", "A" })]
        [TestCase(SortOrderEnum.SortByName, new string[] { "A", "B", "C" })]
        public void ItemsSorting(SortOrderEnum sortOrder, string[] methodNames)
        {
            var document = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestSorting.cs"), null);
            var viewModel = new CodeDocumentViewModel { CodeDocument = document, SortOrder = sortOrder };

            viewModel.CodeDocument = SortHelper.Sort(viewModel);

            var sortingClass = (document.First() as IMembers).Members.First() as CodeClassItem;

            Assert.AreEqual(methodNames[0], sortingClass.Members.First().Name);
            Assert.AreEqual(methodNames[1], sortingClass.Members[1].Name);
            Assert.AreEqual(methodNames[2], sortingClass.Members.Last().Name);

            Assert.AreEqual(sortOrder, viewModel.SortOrder);
        }
    }
}
