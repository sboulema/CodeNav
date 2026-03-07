using System.Windows;

namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestEmptyClass : BaseTest
{
    [Test]
    public async Task ShouldBeVisible()
    {
        var codeItems = await MapToCodeItems("Visibility/TestEmptyClass.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Namespace item should have members
        Assert.That((codeItems.First() as IMembers)?.Members.Any(), Is.True);

        // Inner item should be a class
        var innerClass = (codeItems.First() as IMembers)?.Members.First() as CodeClassItem;
        Assert.That(innerClass?.Kind, Is.EqualTo(CodeItemKindEnum.Class));
        Assert.That(innerClass.Name, Is.EqualTo("CodeNavTestEmptyClass"));

        // Class should be visible
        Assert.That(innerClass.Visibility, Is.EqualTo(Visibility.Visible));

        // Since it does not have members, it should not show the expander symbol
        Assert.That(innerClass.HasMembersVisibility, Is.EqualTo(Visibility.Collapsed));
    }
}
