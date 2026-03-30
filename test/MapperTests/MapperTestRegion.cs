namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestRegion : BaseTest
{
    [Test]
    public async Task TestRegions()
    {
        var codeItems = await MapToCodeItems("Region/TestRegions.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // There should be a single class
        var regionClass = GetFirstClass(namespaceItem);

        // The class should have a function in it
        GetMemberByName<CodeFunctionItem>(regionClass, "OutsideRegionFunction");

        // The class should have a region in it
        var regionR1 = GetMemberByName<CodeRegionItem>(regionClass, "R1");

        // Region R1 should have a nested region R15 with a constant in it
        var regionR15 = GetMemberByName<CodeRegionItem>(regionR1, "R15");

        GetMemberByName<CodeItem>(regionR15, "nestedRegionConstant");

        // Region R1 should have a function Test1 and Test2 in it
        GetMemberByName<CodeItem>(regionR1, "Test1");
        GetMemberByName<CodeItem>(regionR1, "Test2");
    }

    [Test]
    public async Task TestRegionsNoName()
    {
        var codeItems = await MapToCodeItems("Region/TestRegionsNoName.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // There should be a single class
        var regionClass = GetFirstClass(namespaceItem);

        // The class should have a region in it
        GetMemberByName<CodeRegionItem>(regionClass, "Region");
    }

    [Test]
    public async Task TestRegionsSpan()
    {
        var codeItems = await MapToCodeItems("Region/TestRegions.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // There should be a single class
        var regionClass = GetFirstClass(namespaceItem);

        // The class should have a function in it
        GetMemberByName<CodeFunctionItem>(regionClass, "OutsideRegionFunction");

        // The class should have a region in it
        var regionR1 = GetMemberByName<CodeRegionItem>(regionClass, "R1");

        using (Assert.EnterMultipleScope())
        {
            // The region should have correct span for outlining usages
            Assert.That(regionR1.Span.Start, Is.EqualTo(198).Or.EqualTo(209));
            Assert.That(regionR1.Span.End, Is.EqualTo(766).Or.EqualTo(812));
        }
    }
}
