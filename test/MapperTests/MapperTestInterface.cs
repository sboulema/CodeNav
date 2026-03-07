namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestInterface : BaseTest
{
    [Test]
    public async Task TestInterfaceShouldBeOk()
    {
        var codeItems = await MapToCodeItems("Interface/TestInterface.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Namespace item should have 3 members
        Assert.That((codeItems.First() as IMembers)?.Members, Has.Count.EqualTo(3));

        // First item should be an interface
        var innerInterface = (codeItems.First() as IMembers)?.Members.First() as CodeInterfaceItem;

        Assert.That(innerInterface?.Members, Has.Count.EqualTo(3));

        // Second item should be the implementing class
        var implementingClass = (codeItems.First() as IMembers)?.Members[1] as CodeClassItem;

        Assert.That(implementingClass, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(implementingClass.Kind, Is.EqualTo(CodeItemKindEnum.Class));
            Assert.That(implementingClass.Members, Has.Count.EqualTo(3));
        }

        var implementedInterface = implementingClass.Members.Last() as CodeImplementedInterfaceItem;

        Assert.That(implementedInterface, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(implementedInterface.Kind, Is.EqualTo(CodeItemKindEnum.ImplementedInterface));
            Assert.That(implementedInterface.Members, Has.Count.EqualTo(3));
            Assert.That(implementedInterface.Name, Does.Not.StartWith("#"));
        }

        // Items should have proper start lines
        using (Assert.EnterMultipleScope())
        {
            Assert.That(implementedInterface.Members[0].Span.Start, Is.EqualTo(234));
            Assert.That(implementedInterface.Members[1].Span.Start, Is.EqualTo(303));
            Assert.That(implementedInterface.Members[2].Span.Start, Is.EqualTo(622));
        }
    }

    [Test]
    public async Task TestInterfaceInRegionShouldBeOk()
    {
        var codeItems = await MapToCodeItems("Interface/TestInterface.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Namespace item should have 3 members
        Assert.That((codeItems.First() as IMembers)?.Members, Has.Count.EqualTo(3));

        // Third item should be the second implementing class
        var implementingClass = (codeItems.First() as IMembers)?.Members.Last() as CodeClassItem;

        Assert.That(implementingClass, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(implementingClass.Kind, Is.EqualTo(CodeItemKindEnum.Class));
            Assert.That(implementingClass.Members, Has.Count.EqualTo(3));
        }

        var region = implementingClass.Members.Last() as CodeRegionItem;

        var implementedInterface = region?.Members.First() as CodeImplementedInterfaceItem;

        Assert.That(implementedInterface, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(implementedInterface.Kind, Is.EqualTo(CodeItemKindEnum.ImplementedInterface));
            Assert.That(implementedInterface.Members, Has.Count.EqualTo(3));
        }
    }

    [Test]
    public async Task TestInterfaceWithRegion()
    {
        var codeItems = await MapToCodeItems("Interface/TestInterfaceRegion.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Namespace item should have 1 member
        Assert.That((codeItems.First() as IMembers)?.Members, Has.Count.EqualTo(1));

        // First item should be an interface
        var innerInterface = (codeItems.First() as IMembers)?.Members.First() as CodeInterfaceItem;

        Assert.That(innerInterface?.Members, Has.Count.EqualTo(4));

        // Region in interface should have 1 member
        var region = innerInterface.Members[3] as CodeRegionItem;

        Assert.That(region, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(region.Kind, Is.EqualTo(CodeItemKindEnum.Region));
            Assert.That(region.Members, Has.Count.EqualTo(1));
        }
    }
}
