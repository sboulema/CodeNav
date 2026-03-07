namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class TestMethodsWithAttributes : BaseTest
{
    [Test]
    public async Task ShouldBeOk()
    {
        var codeItems = await MapToCodeItems("TestMethodsWithAttributes.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Inner item should be a class
        var innerClass = (codeItems.First() as CodeNamespaceItem)?.Members.First() as CodeClassItem;

        // Class should have a method
        var methodWithAttribute = innerClass?.Members.First() as CodeFunctionItem;

        // Span start should be at the start of the method including attributes
        Assert.That(methodWithAttribute?.Span.Start, Is.EqualTo(73));

        // Identifier span start should be at the start of the method identifier excluding attributes
        Assert.That(methodWithAttribute?.IdentifierSpan.Start, Is.EqualTo(101));
    }
}
