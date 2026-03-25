namespace CodeNav.Test.HelperTests;

[TestFixture]
internal class SortHelperTests : BaseTest
{
    [TestCase(SortOrderEnum.SortByFile, new string[] { "C", "D", "B", "A" })]
    [TestCase(SortOrderEnum.SortByName, new string[] { "A", "B", "C", "D" })]
    [TestCase(SortOrderEnum.SortByType, new string[] { "A", "B", "D", "C" })]
    public async Task ItemsSorting(SortOrderEnum sortOrder, string[] methodNames)
    {
        var codeDocumentViewModel = await MapToCodeDocumentViewModel("Sorting/TestSorting.cs");

        codeDocumentViewModel.SortOrder = sortOrder;

        codeDocumentViewModel.CodeItems = SortHelper.Sort(codeDocumentViewModel.CodeItems, sortOrder);

        var namespaceItem = GetNamespace(codeDocumentViewModel.CodeItems);

        var classItem = GetFirstClass(namespaceItem);

        Assert.That(codeDocumentViewModel.SortOrder, Is.EqualTo(sortOrder));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetMemberAtIndex(classItem, 0).Name, Is.EqualTo(methodNames[0]));
            Assert.That(GetMemberAtIndex(classItem, 1).Name, Is.EqualTo(methodNames[1]));
            Assert.That(GetMemberAtIndex(classItem, 2).Name, Is.EqualTo(methodNames[2]));
            Assert.That(GetMemberAtIndex(classItem, 3).Name, Is.EqualTo(methodNames[3]));
        }
    }
}
