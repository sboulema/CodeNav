namespace CodeNav.Test.HelperTests;

[TestFixture]
internal class HighlightHelperTests : BaseTest
{
    [Test]
    public async Task CurrentItemShouldBeHighlighted()
    {
        var document = await MapToCodeDocumentViewModel("TestProperties.cs");

        HighlightHelper.HighlightCurrentItem(document, 263); // linenumber 13

        var highlightedClass = (document.CodeItems.First() as IMembers)?.Members.First() as CodeClassItem;
        var highlightedItem = highlightedClass?.Members[2];

        Assert.That(highlightedItem?.IsHighlighted, Is.True);
    }

    [Test]
    public async Task OnlyOneItemShouldBeHighlighted()
    {
        var document = await MapToCodeDocumentViewModel("TestProperties.cs");

        HighlightHelper.HighlightCurrentItem(document, 296); // linenumber 15

        HighlightHelper.HighlightCurrentItem(document, 369); // linenumber 20


        var highlightedItems = new List<CodeItem>();
        FindHighlightedItems(highlightedItems, document.CodeItems);

        Assert.That(highlightedItems, Has.Count.EqualTo(1));
    }

    private static void FindHighlightedItems(List<CodeItem> found, IEnumerable<CodeItem> source)
    {
        foreach (var item in source)
        {
            if (item.Kind == CodeItemKindEnum.Property &&
                item.IsHighlighted)
            {
                found.Add(item);
            }

            if (item is IMembers membersCodeItem)
            {
                FindHighlightedItems(found, membersCodeItem.Members);
            }
        }
    }
}
