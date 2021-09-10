using CodeNav.Helpers;
using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CodeNav.Tests.HelperTests
{
    [TestFixture]
    public class HighlightHelperTests
    {
        [Test]
        public void CurrentItemShouldBeHighlighted()
        {
            var document = new CodeDocumentViewModel
            {
                CodeDocument = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestProperties.cs"), null)
            };

            _ = HighlightHelper.HighlightCurrentItem(document, 13, Brushes.Red.Color, Brushes.Blue.Color, Brushes.Green.Color, Brushes.White.Color);

            var highlightedClass = (document.CodeDocument.First() as IMembers).Members.First() as CodeClassItem;
            var highlightedItem = highlightedClass.Members[2];

            Assert.AreEqual(FontWeights.Bold, highlightedItem.FontWeight);
            Assert.AreEqual(Brushes.Red.Color, highlightedItem.ForegroundColor);
            Assert.AreEqual(Brushes.Blue.Color, highlightedItem.NameBackgroundColor);
            Assert.AreEqual(Brushes.Green.Color, highlightedClass.BorderColor);
        }

        [Test]
        public void OnlyOneItemShouldBeHighlighted()
        {
            var document = new CodeDocumentViewModel
            {
                CodeDocument = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestProperties.cs"), null)
            };

            _ = HighlightHelper.HighlightCurrentItem(document, 15, Brushes.Red.Color, Brushes.Blue.Color, Brushes.Green.Color, Brushes.White.Color);

            _ = HighlightHelper.HighlightCurrentItem(document, 20, Brushes.Red.Color, Brushes.Blue.Color, Brushes.Green.Color, Brushes.White.Color);


            var highlightedItems = new List<CodeItem>();
            FindHighlightedItems(highlightedItems, document.CodeDocument);

            Assert.AreEqual(1, highlightedItems.Count);
        }

        private void FindHighlightedItems(List<CodeItem> found, List<CodeItem> source)
        {
            foreach (var item in source)
            {
                if (item.Kind == CodeItemKindEnum.Property && (
                    item.FontWeight == FontWeights.Bold ||
                    item.BackgroundColor == Brushes.Blue.Color ||
                    item.ForegroundColor == Brushes.Red.Color))
                {
                    found.Add(item);
                }

                if (item is IMembers)
                {
                    FindHighlightedItems(found, (item as IMembers).Members);
                }
            }
        }
    }
}
