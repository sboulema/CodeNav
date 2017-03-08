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
            var document = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestProperties.cs"));

            HighlightHelper.HighlightCurrentItem(document, 13, Brushes.Red, Brushes.Blue, Brushes.Green, Brushes.White);

            var highlightedClass = (document.First() as IMembers).Members.First() as CodeClassItem;
            var highlightedItem = highlightedClass.Members[2];

            Assert.AreEqual(FontWeights.Bold, highlightedItem.FontWeight);
            Assert.AreEqual(Brushes.Red, highlightedItem.Foreground);
            Assert.AreEqual(Brushes.Blue, highlightedItem.HighlightBackground);

            Assert.AreEqual(Brushes.Green, highlightedClass.BorderBrush);
        }

        [Test]
        public void OnlyOneItemShouldBeHighlighted()
        {
            var document = SyntaxMapper.MapDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Files\\TestProperties.cs"));

            HighlightHelper.HighlightCurrentItem(document, 13, Brushes.Red, Brushes.Blue, Brushes.Green, Brushes.White);

            HighlightHelper.HighlightCurrentItem(document, 18, Brushes.Red, Brushes.Blue, Brushes.Green, Brushes.White);


            var highlightedItems = new List<CodeItem>();
            FindHighlightedItems(highlightedItems, document);

            Assert.AreEqual(1, highlightedItems.Count);
        }

        private void FindHighlightedItems(List<CodeItem> found, List<CodeItem> source)
        {
            foreach (var item in source)
            {
                if (item.Kind == CodeItemKindEnum.Property && (
                    item.FontWeight == FontWeights.Bold ||
                    item.HighlightBackground == Brushes.Blue ||
                    item.Foreground == Brushes.Red))
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
