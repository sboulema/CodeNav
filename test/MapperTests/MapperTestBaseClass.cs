namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestBaseClass : BaseTest
{
    [Test]
    public async Task TestInlineBaseClassShouldBeOk()
    {
        var codeItems = await MapToCodeItems("BaseClass/TestBaseClass.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // Namespace item should have 3 members
        Assert.That(namespaceItem.Members, Has.Count.EqualTo(3));

        // Second item should be the derived class
        var derivedClass = GetSecondClass(namespaceItem);

        // Derived class should have 2 members (1 property + 1 base class region)
        Assert.That(derivedClass.Members, Has.Count.EqualTo(2));

        var derivedClassProperty = GetMemberAtIndex(derivedClass, 0);

        var baseClassRegion = derivedClass.Members.Last() as CodeRegionItem;

        Assert.That(baseClassRegion, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(baseClassRegion.Kind, Is.EqualTo(CodeItemKindEnum.BaseClass));
            Assert.That(baseClassRegion.Members, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public async Task TestExternalBaseClassShouldBeOk()
    {
        var codeItems = await MapToCodeItems("BaseClass/TestBaseClass.cs");

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Namespace item should have 3 members
        Assert.That((codeItems.First() as IMembers)?.Members, Has.Count.EqualTo(3));

        // First item should be the derived class
        var derivedClass = (codeItems.First() as IMembers)?.Members[0] as CodeClassItem;

        Assert.That(derivedClass, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(derivedClass.Kind, Is.EqualTo(CodeItemKindEnum.Class));
            Assert.That(derivedClass.Members, Has.Count.EqualTo(2));
        }
    }
}
