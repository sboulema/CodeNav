using System.Windows;

namespace CodeNav.Test.HelperTests;

[TestFixture]
internal class VisibilityHelperTests : BaseTest
{
    [TestCase(false, true, Visibility.Visible)] // isEmpty: false, hide: true, does not apply since the class is empty
    [TestCase(false, false, Visibility.Visible)] // isEmpty: false, hide: false, does not apply since the class is empty
    [TestCase(true, false, Visibility.Visible)] // isEmpty: true, hide: false, applies but does not hide
    [TestCase(true, true, Visibility.Collapsed)] // isEmpty: true, hide: true, applies and hides class
    public async Task FilterRuleModifierIsEmptyActionHide_ShouldWorkOk(bool isEmpty, bool hide, Visibility expectedVisibility)
    {
        var codeDocumentViewModel = new CodeDocumentViewModel
        {
            FilterRules =
            [
                new()
                {
                    Access = CodeItemAccessEnum.All,
                    Kind = CodeItemKindEnum.Class,
                    IsEmpty = isEmpty,
                    Hide = hide,
                }
            ]
        };

        codeDocumentViewModel = await MapToCodeDocumentViewModel("Visibility/TestVisibility.cs", codeDocumentViewModel);

        VisibilityHelper.SetCodeItemVisibility(codeDocumentViewModel.CodeItems, codeDocumentViewModel.FilterRules);

        var firstClass = (codeDocumentViewModel.CodeItems.First() as IMembers)?.Members.First() as CodeClassItem;          

        Assert.That(firstClass?.Visibility, Is.EqualTo(expectedVisibility));
    }

    // Namespace: visible, Class: visible, IsEqual: visible, IsGreater: collapsed
    [TestCase("IsE", Visibility.Visible, Visibility.Visible, Visibility.Visible, Visibility.Collapsed)]
    // Namespace: visible, Class: visible, IsEqual: visible, IsGreater: collapsed [Case-Insensitive]
    [TestCase("Ise", Visibility.Visible, Visibility.Visible, Visibility.Visible, Visibility.Collapsed)]
    // Namespace: visible, Class: visible, IsEqual: collapsed, IsGreater: visible
    [TestCase("IsG", Visibility.Visible,  Visibility.Visible, Visibility.Collapsed, Visibility.Visible)]
    // Namespace: visible, Class: visible, IsEqual: visible, IsGreater: visible
    [TestCase("Is", Visibility.Visible, Visibility.Visible, Visibility.Visible, Visibility.Visible)]
    // Namespace: collapsed, Class: collapsed, IsEqual: collapsed, IsGreater: collapsed [No visible members]
    [TestCase("K", Visibility.Collapsed, Visibility.Collapsed, Visibility.Collapsed, Visibility.Collapsed)]
    public async Task FilterText_ShouldWorkOk(
        string filterText,
        Visibility expectedNamespaceVisibility,
        Visibility expectedClassVisibility,
        Visibility expectedFirstMemberVisibility,
        Visibility expectedSecondMemberVisibility)
    {
        var codeDocumentViewModel = await MapToCodeDocumentViewModel("Visibility/TestVisibilityByFilterText.cs");

        codeDocumentViewModel.FilterText = filterText;

        VisibilityHelper.SetCodeItemVisibility(codeDocumentViewModel.CodeItems, codeDocumentViewModel.FilterRules, codeDocumentViewModel.FilterText);

        var namespaceItem = GetNamespace(codeDocumentViewModel.CodeItems);

        Assert.That(namespaceItem?.Visibility, Is.EqualTo(expectedNamespaceVisibility));

        var classItem = GetFirstClass(namespaceItem);

        Assert.That(classItem?.Visibility, Is.EqualTo(expectedClassVisibility));

        var firstMember = GetMemberAtIndex(classItem, 0);

        Assert.That(firstMember?.Visibility, Is.EqualTo(expectedFirstMemberVisibility));

        var secondMember = GetMemberAtIndex(classItem, 1);

        Assert.That(secondMember?.Visibility, Is.EqualTo(expectedSecondMemberVisibility));
    }

    [TestCase(false, Visibility.Visible)]
    [TestCase(true, Visibility.Collapsed)]
    public async Task FilterRuleIgnore_Namespace_ShouldWorkOk(bool ignore, Visibility expectedVisibility)
    {
        var codeDocumentViewModel = new CodeDocumentViewModel
        {
            FilterRules =
            [
                new()
                {
                    Access = CodeItemAccessEnum.All,
                    Kind = CodeItemKindEnum.Namespace,
                    Ignore = ignore,
                }
            ]
        };

        codeDocumentViewModel = await MapToCodeDocumentViewModel("Visibility/TestVisibility.cs", codeDocumentViewModel);

        VisibilityHelper.SetCodeItemVisibility(codeDocumentViewModel.CodeItems, codeDocumentViewModel.FilterRules);

        var namespaceItem = GetNamespace(codeDocumentViewModel.CodeItems);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(namespaceItem.IgnoreVisibility, Is.EqualTo(expectedVisibility));
            Assert.That(namespaceItem.NotIgnoreVisibility, Is.Not.EqualTo(expectedVisibility));
        }
    }
}
