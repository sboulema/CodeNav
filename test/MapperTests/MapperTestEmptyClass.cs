using System.Windows;

namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestEmptyClass : BaseTest
{
    [Test]
    public async Task ShouldBeVisible()
    {
        var codeItems = await MapToCodeItems("Visibility/TestEmptyClass.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // Inner item should be a class
        var classItem = GetFirstClass(namespaceItem);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(classItem.Name, Is.EqualTo("CodeNavTestEmptyClass"));

            // Class should be visible
            Assert.That(classItem.Visibility, Is.EqualTo(Visibility.Visible));

            // Since it does not have members, it should not show the expander symbol
            Assert.That(classItem.HasMembersVisibility, Is.EqualTo(Visibility.Collapsed));
        }
    }
}
