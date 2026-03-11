namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestMethodsWithAttributes : BaseTest
{
    [Test]
    public async Task ShouldBeOk()
    {
        var codeItems = await MapToCodeItems("Method/TestMethodsWithAttributes.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // Inner item should be a class
        var classItem = GetFirstClass(namespaceItem);

        // Class should have a method
        var methodWithAttribute = GetMemberAtIndex(classItem, 0) as CodeFunctionItem;

        using (Assert.EnterMultipleScope())
        {
            // Span start should be at the start of the method including attributes
            Assert.That(methodWithAttribute?.Span.Start, Is.EqualTo(76).Or.EqualTo(80));

            // Identifier span start should be at the start of the method identifier excluding attributes
            Assert.That(methodWithAttribute?.IdentifierSpan.Start, Is.EqualTo(103).Or.EqualTo(108));
        }
    }
}
