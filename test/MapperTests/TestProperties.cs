namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class TestProperties : BaseTest
{
    [Test]
    public async Task ShouldBeOk()
    {
        var codeItems = await MapToCodeItems("TestProperties.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Namespace item should have 1 member
        Assert.That((codeItems.First() as IMembers)?.Members.Count, Is.EqualTo(1));

        // Inner item should be a class
        var innerClass = (codeItems.First() as IMembers)?.Members.First() as CodeClassItem;

        // Inheriting class should have properties
        var propertyGetSet = innerClass?.Members.First() as CodeFunctionItem;
        Assert.That(propertyGetSet?.Parameters, Is.EqualTo(" {get,set}"));

        var propertyGet = innerClass?.Members[1] as CodeFunctionItem;
        Assert.That(propertyGet?.Parameters, Is.EqualTo(" {get}"));

        var propertySet = innerClass?.Members[2] as CodeFunctionItem;
        Assert.That(propertySet?.Parameters, Is.EqualTo(" {set}"));

        var property = innerClass?.Members.Last() as CodeFunctionItem;
        Assert.That(property?.Parameters, Is.Empty);
    }
}
