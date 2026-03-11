namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestMethodsWithComments : BaseTest
{
    [Test]
    public async Task ShouldBeOk()
    {
        var codeItems = await MapToCodeItems("Method/TestMethodsWithComments.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // Inner item should be a class
        var classItem = GetFirstClass(namespaceItem);

        // Class should have a method
        var methodWithComment = GetMemberAtIndex(classItem, 0) as CodeFunctionItem;

        Assert.That(methodWithComment?.Tooltip, Is.EqualTo($"Public void MethodWithComment (){Environment.NewLine}{Environment.NewLine}Super important summary"));

        // Class should have a method
        var methodWithoutComment = GetMemberAtIndex(classItem, 1) as CodeFunctionItem;

        Assert.That(methodWithoutComment?.Tooltip, Is.EqualTo("Public void MethodWithoutComment ()"));

        // Class should have a method
        var methodWithMultipleComment = GetMemberAtIndex(classItem, 2) as CodeFunctionItem;

        Assert.That(methodWithMultipleComment?.Tooltip, Is.EqualTo($"Public void MethodWithMultipleComment (){Environment.NewLine}{Environment.NewLine}Multiple comment - summary"));

        // Class should have a method
        var methodWithReorderedComment = GetMemberAtIndex(classItem, 3) as CodeFunctionItem;

        Assert.That(methodWithReorderedComment?.Tooltip, Is.EqualTo($"Public void MethodWithReorderedComment (){Environment.NewLine}{Environment.NewLine}Multiple comment - summary"));
    }
}
