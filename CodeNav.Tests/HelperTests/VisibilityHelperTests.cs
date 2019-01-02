using CodeNav.Helpers;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace CodeNav.Tests.HelperTests
{
    [TestFixture]
    public class VisibilityHelperTests
    {
        [TestCase(false, Visibility.Visible)]
        [TestCase(true, Visibility.Collapsed)]
        public void EmptyItemsShouldRespectSetting(bool hideItemsWithoutChildren, Visibility expectedVisibility)
        {
            var document = new CodeDocumentViewModel
            {
                CodeDocument = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestVisibility.cs"))
            };

            SettingsHelper.HideItemsWithoutChildren = hideItemsWithoutChildren;

            VisibilityHelper.SetCodeItemVisibility(document.CodeDocument);

            var firstClass = (document.CodeDocument.First() as IMembers).Members.First() as CodeClassItem;          

            Assert.AreEqual(expectedVisibility, firstClass.IsVisible);
        }
    }
}
