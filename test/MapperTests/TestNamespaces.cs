namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class TestNamespaces : BaseTest
{
    [Test]
    public async Task NestedNamespacesShouldHaveCorrectStructure()
    {
        var codeItems = await MapToCodeItems("TestNestedNamespaces.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Namespace item should have members
        Assert.That((codeItems.First() as IMembers)?.Members.Any(), Is.True);

        // Inner item should also be a namespace
        var innerNamespace = (codeItems.First() as IMembers)?.Members.First() as CodeNamespaceItem;
        Assert.That(innerNamespace?.Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // That inner namespace should have members
        Assert.That(innerNamespace.Members.Any(), Is.True);

        // That member should be a class
        var innerClass = (innerNamespace as IMembers).Members.First() as CodeClassItem;
        Assert.That(innerClass?.Kind, Is.EqualTo(CodeItemKindEnum.Class));
        Assert.That(innerClass.Name, Is.EqualTo("ClassInNestedNamespace"));
    }
}
