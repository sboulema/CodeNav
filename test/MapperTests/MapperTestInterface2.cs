namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestInterface2 : BaseTest
{
    [Test]
    public async Task TestNestedInterfaceShouldBeOk()
    {
        var codeItems = await MapToCodeItems("Interface/TestInterface2.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Namespace item should have 3 members
        Assert.That((codeItems.First() as IMembers)?.Members, Has.Count.EqualTo(3));

        // Second item should be the implementing class
        var implementingClass = (codeItems.First() as IMembers)?.Members.Last() as CodeClassItem;

        Assert.That(implementingClass, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(implementingClass.Kind, Is.EqualTo(CodeItemKindEnum.Class));
            Assert.That(implementingClass.Members, Has.Count.EqualTo(2));
        }

        var implementedInterface1 = implementingClass.Members.First() as CodeImplementedInterfaceItem;

        Assert.That(implementedInterface1, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(implementedInterface1?.Kind, Is.EqualTo(CodeItemKindEnum.ImplementedInterface));
            Assert.That(implementedInterface1.Members, Has.Count.EqualTo(1));
        }

        var implementedInterface2 = implementingClass.Members.Last() as CodeImplementedInterfaceItem;

        Assert.That(implementedInterface2, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(implementedInterface2?.Kind, Is.EqualTo(CodeItemKindEnum.ImplementedInterface));
            Assert.That(implementedInterface2.Members, Has.Count.EqualTo(1));
        }
    }
}
