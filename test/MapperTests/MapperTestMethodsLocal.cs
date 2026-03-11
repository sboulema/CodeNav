namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class MapperTestMethodsLocal : BaseTest
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
        var codeItems = await MapToCodeItems("Method/TestMethodsLocal.cs");

        // First item should be a namespace
        var namespaceItem = GetNamespace(codeItems);

        // Inner item should be a class
        var classItem = GetFirstClass(namespaceItem);

        // Class should have a method
        var method = GetMemberAtIndex(classItem, 0) as CodeClassItem;

        Assert.That(method?.Name, Is.EqualTo("Method"));

        // Method should have a local method
        var localMethod = method.Members.First() as CodeFunctionItem;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(localMethod?.Name, Is.EqualTo("LocalMethod"));

            // Local method should have a proper starting line
            Assert.That(localMethod.Span.Start, Is.EqualTo(179));
        }
    }
}
