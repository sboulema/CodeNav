namespace CodeNav.Test.HelperTests;

[TestFixture]
internal class SortHelperTests : BaseTest
{
    [TestCase(SortOrderEnum.SortByFile, new string[] { "C", "B", "A" })]
    [TestCase(SortOrderEnum.SortByName, new string[] { "A", "B", "C" })]
    public async Task ItemsSorting(SortOrderEnum sortOrder, string[] methodNames)
    {
        var document = await MapToCodeDocumentViewModel("TestSorting.cs");

        document.SortOrder = sortOrder;

        document.CodeItems = SortHelper.Sort(document.CodeItems, sortOrder);

        var sortingClass = (document.CodeItems.First() as IMembers)?.Members.First() as CodeClassItem;

        Assert.That(sortingClass?.Members.First().Name, Is.EqualTo(methodNames[0]));
        Assert.That(sortingClass.Members[1].Name, Is.EqualTo(methodNames[1]));
        Assert.That(sortingClass.Members.Last().Name, Is.EqualTo(methodNames[2]));

        Assert.That(document.SortOrder, Is.EqualTo(sortOrder));
    }
}
