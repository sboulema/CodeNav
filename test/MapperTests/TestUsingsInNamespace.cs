namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class TestUsingsInNamespace : BaseTest
{
    [Test]
    public async Task TestUsingsInNamespaceShouldBeOk()
    {
        var codeItems = await MapToCodeItems("TestUsingsInNamespace.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Namespace item should have 1 member
        Assert.That((codeItems.First() as IMembers)?.Members, Has.Count.EqualTo(1));

        // Item should be a class
        var innerClass = (codeItems.First() as IMembers)?.Members.First() as CodeClassItem;

        Assert.That(innerClass?.Kind, Is.EqualTo(CodeItemKindEnum.Class));
        Assert.That(innerClass.Members, Has.Count.EqualTo(2));

        Assert.That(innerClass.Members.First().Kind, Is.EqualTo(CodeItemKindEnum.Constructor));
        Assert.That(innerClass.Members.Last().Kind, Is.EqualTo(CodeItemKindEnum.Method));
    }
}
