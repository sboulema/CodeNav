namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class TestIndexers : BaseTest
{
    [Test]
    public async Task ShouldBeOk()
    {
        var codeItems = await MapToCodeItems("TestIndexers.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Inner item should be a class
        var innerClass = (codeItems.First() as CodeNamespaceItem)?.Members.First() as CodeClassItem;

        // Class should have an indexer
        var indexer = innerClass?.Members.First() as CodeFunctionItem;

        Assert.That(indexer?.Kind, Is.EqualTo(CodeItemKindEnum.Indexer));
    }
}
