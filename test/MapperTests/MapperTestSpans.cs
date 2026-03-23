namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestSpans : BaseTest
{
    [Test]
    public async Task SpanShouldBeOk()
    {
        var codeItems = await MapToCodeItems("TestSpans.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // First member should be the class
        var outlineClass = GetFirstClass(namespaceItem);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(outlineClass.Span.Start, Is.EqualTo(60));
            Assert.That(outlineClass.Span.End, Is.EqualTo(120));
        }
    }

    [Test]
    public async Task IdentifierSpanShouldBeOk()
    {
        var codeItems = await MapToCodeItems("TestSpans.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // First member should be the class
        var outlineClass = GetFirstClass(namespaceItem);

        Assert.That(outlineClass.IdentifierSpan, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(outlineClass.IdentifierSpan.Value.Start, Is.EqualTo(75));
            Assert.That(outlineClass.IdentifierSpan.Value.End, Is.EqualTo(84));
        }
    }

    [Test]
    public async Task OutlineSpanShouldBeOk()
    {
        var codeItems = await MapToCodeItems("TestSpans.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // First member should be the class
        var outlineClass = GetFirstClass(namespaceItem);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(outlineClass.OutlineSpan.Start, Is.EqualTo(84));
            Assert.That(outlineClass.OutlineSpan.End, Is.EqualTo(120));
        }
    }

    [Test]
    public async Task OutlineSpanSecondClassShouldBeOk()
    {
        var codeItems = await MapToCodeItems("TestSpans.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // Second member should be the class
        var outlineClass = GetSecondClass(namespaceItem);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(outlineClass.OutlineSpan.Start, Is.EqualTo(149));
            Assert.That(outlineClass.OutlineSpan.End, Is.EqualTo(200));
        }
    }
}
