namespace CodeNav.Test.MapperTests;

[TestFixture]
internal class TestSealed : BaseTest
{
    [Test]
    public async Task TestSealedShouldBeOk()
    {
        var codeItems = await MapToCodeItems("TestSealed.cs");

        Assert.That(codeItems.Any(), Is.True);

        // First item should be a namespace
        Assert.That(codeItems.First().Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        // Namespace item should have 3 members
        Assert.That((codeItems.First() as IMembers)?.Members, Has.Count.EqualTo(3));

        // Inner item should be a sealed class
        var sealedBaseClass = (codeItems.First() as IMembers)?.Members.First() as CodeClassItem;
        
        Assert.That(sealedBaseClass?.Kind, Is.EqualTo(CodeItemKindEnum.Class));
        Assert.That(sealedBaseClass.Name, Is.EqualTo("SealedBaseClass"));
        Assert.That(sealedBaseClass.Access, Is.EqualTo(CodeItemAccessEnum.Sealed));

        // Inheriting Class should be there
        var inheritingClass = (codeItems.First() as IMembers)?.Members.Last() as CodeClassItem;

        Assert.That(inheritingClass?.Kind, Is.EqualTo(CodeItemKindEnum.Class));
        Assert.That(inheritingClass.Name, Is.EqualTo("InheritingClass"));
        Assert.That(inheritingClass.Access, Is.EqualTo(CodeItemAccessEnum.Public));
        Assert.That(inheritingClass.Parameters, Is.EqualTo(" : NonSealedBaseClass"));

        // Inheriting class should have sealed property
        var sealedProperty = inheritingClass.Members.First(codeItem => codeItem is CodePropertyItem) as CodeFunctionItem;

        Assert.That(sealedProperty?.Kind, Is.EqualTo(CodeItemKindEnum.Property));
        Assert.That(sealedProperty.Name, Is.EqualTo("BaseProperty"));
        Assert.That(sealedProperty.Access, Is.EqualTo(CodeItemAccessEnum.Sealed));
        Assert.That(sealedProperty.Parameters, Is.EqualTo(" {get,set}"));
    }
}
