namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestInterface3 : BaseTest
{
    [Test]
    public async Task TestBaseImplementedInterfaceShouldBeOk()
    {
        var codeItems = await MapToCodeItems("Interface/TestInterface3.cs");

        Assert.That(codeItems.Any(), Is.True);

        // last item should be the class "ImplementingClass3", that implements the base class "BaseClass"
        var implementingClass = (codeItems.First() as IMembers)?.Members.Last() as CodeClassItem;

        Assert.That(implementingClass, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(implementingClass.Kind, Is.EqualTo(CodeItemKindEnum.Class));
            Assert.That(implementingClass.Members, Has.Count.EqualTo(2));
        }

        var implementedInterface = implementingClass.Members.First() as CodeImplementedInterfaceItem;

        Assert.That(implementedInterface, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(implementedInterface.Kind, Is.EqualTo(CodeItemKindEnum.ImplementedInterface));
            Assert.That(implementedInterface.Members, Has.Count.EqualTo(1));
        }
    }
}
