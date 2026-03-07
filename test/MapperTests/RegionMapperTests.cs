namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class RegionMapperTests : BaseTest
{
    [Test]
    public async Task TestRegions()
    {
        var codeItems = await MapToCodeItems("TestRegions.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // There should be a single class
        var regionClass = (codeItems.First() as IMembers)?.Members.First() as CodeClassItem;
        Assert.That(regionClass, Is.Not.Null);

        // The class should have a function in it
        Assert.That(regionClass.Members.FirstOrDefault(m => m.Name.Equals("OutsideRegionFunction")), Is.Not.Null);

        // The class should have a region in it
        var regionR1 = regionClass.Members.FirstOrDefault(m => m.Name.Equals("#R1")) as CodeRegionItem;
        Assert.That(regionR1, Is.Not.Null, "Region #R1 not found");

        // Region R1 should have a nested region R15 with a constant in it
        var regionR15 = regionR1.Members.FirstOrDefault(m => m.Name.Equals("#R15")) as CodeRegionItem;
        Assert.That(regionR15, Is.Not.Null);
        Assert.That(regionR15.Members.FirstOrDefault(m => m.Name.Equals("nestedRegionConstant")), Is.Not.Null);

        // Region R1 should have a function Test1 and Test2 in it
        Assert.That(regionR1.Members.FirstOrDefault(m => m.Name.Equals("Test1")), Is.Not.Null);
        Assert.That(regionR1.Members.FirstOrDefault(m => m.Name.Equals("Test2")), Is.Not.Null);
    }

    [Test]
    public async Task TestRegionsNoName()
    {
        var codeItems = await MapToCodeItems("TestRegionsNoName.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // There should be a single class
        var regionClass = (codeItems.First() as IMembers)?.Members.First() as CodeClassItem;
        Assert.That(regionClass, Is.Not.Null);

        // The class should have a region in it
        var regionR1 = regionClass.Members.FirstOrDefault(m => m.Name.Equals("#Region")) as CodeRegionItem;
        Assert.That(regionR1, Is.Not.Null, "Region #Region not found");
    }

    [Test]
    public async Task TestRegionsSpan()
    {
        var codeItems = await MapToCodeItems("TestRegions.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // There should be a single class
        var regionClass = (codeItems.First() as IMembers)?.Members.First() as CodeClassItem;
        Assert.That(regionClass, Is.Not.Null);

        // The class should have a function in it
        Assert.That(regionClass.Members.FirstOrDefault(m => m.Name.Equals("OutsideRegionFunction")), Is.Not.Null);

        // The class should have a region in it
        var regionR1 = regionClass.Members.FirstOrDefault(m => m.Name.Equals("#R1")) as CodeRegionItem;
        Assert.That(regionR1, Is.Not.Null, "Region #R1 not found");

        // The region should have correct span for outlining usages
        Assert.That(regionR1.Span.Start, Is.EqualTo(101));
        Assert.That(regionR1.Span.End, Is.EqualTo(704));
    }
}
