namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class TestMethodsLocal : BaseTest
{
    /// <summary>
    /// Check if a local function inside a method is mapped correctly.
    /// </summary>
    /// <remarks>
    /// https://github.com/sboulema/CodeNav/issues/107
    /// </remarks>
    /// <returns></returns>
    [Test]
    public async Task ShouldBeOk()
    {
        var codeItems = await MapToCodeItems("TestMethodsLocal.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Inner item should be a class
        var innerClass = (codeItems.First() as CodeNamespaceItem)?.Members.First() as CodeClassItem;

        // Class should have a method
        var method = innerClass?.Members.First() as CodeClassItem;

        Assert.That(method?.Name, Is.EqualTo("Method"));

        // Method should have a local method
        var localMethod = method.Members.First() as CodeFunctionItem;

        Assert.That(localMethod?.Name, Is.EqualTo("LocalMethod"));

        // Local method should have a proper starting line
        Assert.That(localMethod.Span.Start, Is.EqualTo(180));
    }
}
