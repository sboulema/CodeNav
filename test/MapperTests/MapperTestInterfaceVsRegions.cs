namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestInterfaceVsRegions : BaseTest
{
    [Test]
    public async Task TestInterfaceVsRegions_FirstImplementationShouldBeOk()
    {
        var codeItems = await MapToCodeItems("Interface/TestInterfacesVsRegions.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // First member should be a class
        var classItem = GetFirstClass(namespaceItem);

        // First class member should be a region
        var regionItem = GetMemberAtIndex(classItem, 0) as CodeRegionItem;

        // First region member should be an implemented interface
        var implementedInterface = regionItem?.Members.First() as CodeImplementedInterfaceItem;

        // Implemented interface should have 1 member
        Assert.That(implementedInterface?.Members, Has.Count.EqualTo(1));

        // Member should have name "PublicMethod1"
        var memberItem = implementedInterface?.Members.First();

        Assert.That(memberItem?.Name, Is.EqualTo("PublicMethod1"));
    }

    [Test]
    public async Task TestInterfaceVsRegions_SecondImplementationShouldBeOk()
    {
        var codeItems = await MapToCodeItems("Interface/TestInterfacesVsRegions.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // First member should be a class
        var classItem = GetFirstClass(namespaceItem);

        // Second class member should be a region
        var regionItem = GetMemberAtIndex(classItem, 1) as CodeRegionItem;

        // First region member should be an implemented interface
        var implementedInterface = regionItem?.Members.First() as CodeImplementedInterfaceItem;

        // Implemented interface should have 1 member
        Assert.That(implementedInterface?.Members, Has.Count.EqualTo(1));

        // Member should have name "PublicMethod2"
        var memberItem = implementedInterface?.Members.First();

        Assert.That(memberItem?.Name, Is.EqualTo("PublicMethod2"));
    }
}
