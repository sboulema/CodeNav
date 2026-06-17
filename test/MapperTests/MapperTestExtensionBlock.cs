namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestExtensionBlock : BaseTest
{
    [Test]
    public async Task ExtensionBlockShouldBeOk()
    {
        var codeItems = await MapToCodeItems("ExtensionBlock/TestExtensionBlock.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        var classItem = GetFirstClass(namespaceItem);

        var extensionBlockItem = GetMemberAtIndex(classItem, 1) as CodeClassItem;

        // Extension block item should have 2 members
        Assert.That(extensionBlockItem!.Members, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task RegionInsideExtensionBlockShouldNotBeDuplicated()
    {
        // Regression test for https://github.com/sboulema/CodeNav/issues/180
        var codeItems = await MapToCodeItems("ExtensionBlock/TestExtensionBlockRegion.cs");

        var namespaceItem = GetNamespace(codeItems);
        var classItem = GetFirstClass(namespaceItem);

        // The class should only have the extension block as a member, the region
        // inside the extension block should not also show up as a sibling of it
        Assert.That(classItem.Members, Has.Count.EqualTo(1));

        var extensionBlockItem = GetMemberAtIndex(classItem, 0) as CodeClassItem;

        // The extension block should have a single member: the "Foo" region
        Assert.That(extensionBlockItem!.Members, Has.Count.EqualTo(1));

        var regionItem = GetFirstRegion(extensionBlockItem);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(regionItem.Name, Is.EqualTo("Foo"));

            // The region should contain the IsNull method
            GetMemberByName<CodeItem>(regionItem, "IsNull");
        }
    }
}
