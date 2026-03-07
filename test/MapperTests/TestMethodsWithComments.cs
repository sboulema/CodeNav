namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class TestMethodsWithComments : BaseTest
{
    [Test]
    public async Task ShouldBeOk()
    {
        var codeItems = await MapToCodeItems("TestMethodsWithComments.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Inner item should be a class
        var innerClass = (codeItems.First() as CodeNamespaceItem)?.Members.First() as CodeClassItem;

        // Class should have a method
        var methodWithComment = innerClass?.Members.First() as CodeFunctionItem;

        Assert.That(methodWithComment?.Tooltip, Is.EqualTo("Super important summary"));

        // Class should have a method
        var methodWithoutComment = innerClass?.Members[1] as CodeFunctionItem;

        Assert.That(methodWithoutComment?.Tooltip, Is.EqualTo("Public MethodWithoutComment ()"));

        // Class should have a method
        var methodWithMultipleComment = innerClass?.Members[2] as CodeFunctionItem;

        Assert.That(methodWithMultipleComment?.Tooltip, Is.EqualTo("Multiple comment - summary"));

        // Class should have a method
        var methodWithReorderedComment = innerClass?.Members[3] as CodeFunctionItem;

        Assert.That(methodWithReorderedComment?.Tooltip, Is.EqualTo("Multiple comment - summary"));
    }
}
