namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class TestNoNamespace : BaseTest
{
    [Test]
    public async Task ShouldHaveCorrectStructure()
    {
        var codeItems = await MapToCodeItems("TestNoNamespace.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a class
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Class));
    }
}
