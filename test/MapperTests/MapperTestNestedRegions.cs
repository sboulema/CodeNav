namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestNestedRegions : BaseTest
{
    [Test]
    public async Task NestedRegionsShouldWork()
    {
        var codeItems = await MapToCodeItems("Region/TestNestedRegions.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // Namespace should have 1 member
        Assert.That(namespaceItem.Members, Has.Count.EqualTo(1));

        // Inner item should be a class
        var classItem = GetFirstClass(namespaceItem);

        // That inner class should have members
        Assert.That(classItem.Members.Any(), Is.True);

        // That member should be a region
        var parentRegion = GetFirstRegion(classItem);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parentRegion.Name, Is.EqualTo("ParentRegion"));

            // That parent region should have members
            Assert.That(parentRegion.Members.Any(), Is.True);
        }

        // That member should be a region
        var innerRegion = GetFirstRegion(parentRegion);

        Assert.That(innerRegion.Name, Is.EqualTo("ChildRegion"));
    }
}
