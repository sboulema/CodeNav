namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class TestNestedRegions : BaseTest
{
    [Test]
    public async Task NestedRegionsShouldWork()
    {
        var codeItems = await MapToCodeItems("TestNestedRegions.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Namespace item should have members
        Assert.That((codeItems.First() as IMembers)?.Members.Any(), Is.True);

        // Namespace should have 1 member
        Assert.That((codeItems.First() as IMembers)?.Members, Has.Count.EqualTo(1));

        // Inner item should be a class
        var innerClass = (codeItems.First() as IMembers)?.Members.First() as CodeClassItem;
        Assert.That(innerClass?.Kind, Is.EqualTo(CodeItemKindEnum.Class));

        // That inner class should have members
        Assert.That(innerClass.Members.Any(), Is.True);

        // That member should be a region
        var parentRegion = (innerClass as IMembers).Members.First() as CodeRegionItem;
        Assert.That(parentRegion?.Kind, Is.EqualTo(CodeItemKindEnum.Region));
        Assert.That(parentRegion.Name, Is.EqualTo("#ParentRegion"));

        // That parent region should have members
        Assert.That(parentRegion.Members.Any(), Is.True);

        // That member should be a region
        var innerRegion = (parentRegion as IMembers).Members.First() as CodeRegionItem;
        Assert.That(innerRegion?.Kind, Is.EqualTo(CodeItemKindEnum.Region));
        Assert.That(innerRegion.Name, Is.EqualTo("#ChildRegion"));
    }
}
