namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestExtensionBlock : BaseTest
{
    [Test]
    public async Task TestInlineBaseClassShouldBeOk()
    {
        var codeItems = await MapToCodeItems("ExtensionBlock/TestExtensionBlock.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        var classItem = GetFirstClass(namespaceItem);

        var extensionBlockItem = GetMemberAtIndex(classItem, 1) as CodeClassItem;

        // Extension block item should have 2 members
        Assert.That(extensionBlockItem!.Members, Has.Count.EqualTo(2));
    }
}
