namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class TestSwitch : BaseTest
{
    [Test]
    public async Task TestSwitchShouldBeOk()
    {
        var codeItems = await MapToCodeItems("TestSwitch.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Namespace item should have 1 member
        Assert.That((codeItems.First() as IMembers)?.Members, Has.Count.EqualTo(1));

        // Inner item should be a class
        var innerClass = (codeItems.First() as IMembers)?.Members.First() as CodeClassItem;
        Assert.That(innerClass?.Kind, Is.EqualTo(CodeItemKindEnum.Class));

        // Class should have 3 methods
        Assert.That(innerClass.Members, Has.Count.EqualTo(3));

        // First method should have a switch inside
        var method = innerClass.Members.First();
        var switchStatement = (method as IMembers)?.Members.First();
        Assert.That(switchStatement?.Kind, Is.EqualTo(CodeItemKindEnum.Switch));

        // Second method should have a switch inside
        var secondMethod = innerClass.Members[1];
        var secondSwitchStatement = (secondMethod as IMembers)?.Members.First();
        Assert.That(secondSwitchStatement?.Kind, Is.EqualTo(CodeItemKindEnum.Switch));

        // last method should have a switch inside
        var lastMethod = innerClass.Members.Last();
        var lastSwitchStatement = (lastMethod as IMembers)?.Members.First();
        Assert.That(lastSwitchStatement?.Kind, Is.EqualTo(CodeItemKindEnum.Switch));
    }
}
