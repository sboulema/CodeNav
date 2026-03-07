namespace CodeNav.Test;

internal abstract class BaseTest
{
    internal static async Task<IEnumerable<CodeItem>> MapToCodeItems(
        string fileName,
        CodeDocumentViewModel? codeDocumentViewModel = null)
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\Files", fileName);
        var fileText = await File.ReadAllTextAsync(filePath);

        var codeItems = await DocumentMapper.MapDocument(
            fileText,
            codeDocumentViewModel ?? new(),
            filePaths: [],
            cancellationToken: default);

        Assert.That(codeItems.Any(), Is.True);

        return codeItems;
    }

    internal static async Task<CodeDocumentViewModel> MapToCodeDocumentViewModel(
        string fileName,
        CodeDocumentViewModel? codeDocumentViewModel = null)
    {
        codeDocumentViewModel ??= new CodeDocumentViewModel();

        var codeItems = await MapToCodeItems(fileName, codeDocumentViewModel);
        codeDocumentViewModel.CodeItems.AddRange(codeItems);
        return codeDocumentViewModel;
    }

    internal static CodeNamespaceItem GetNamespace(IEnumerable<CodeItem> codeItems)
    {
        var namespaceItem = codeItems.First() as CodeNamespaceItem;

        Assert.That(namespaceItem, Is.Not.Null);
        Assert.That(namespaceItem.Kind, Is.EqualTo(CodeItemKindEnum.Namespace));

        return namespaceItem;
    }

    internal static CodeClassItem GetFirstClass(CodeNamespaceItem namespaceItem)
        => GetClassAtIndex(namespaceItem, 0);

    internal static CodeClassItem GetSecondClass(CodeNamespaceItem namespaceItem)
        => GetClassAtIndex(namespaceItem, 1);

    internal static CodeClassItem GetClassAtIndex(CodeNamespaceItem namespaceItem, int index)
    {
        var classItem = namespaceItem.Members[index] as CodeClassItem;

        Assert.That(classItem, Is.Not.Null);
        Assert.That(classItem.Kind, Is.EqualTo(CodeItemKindEnum.Class));

        return classItem;
    }

    internal static CodeItem GetMemberAtIndex(CodeClassItem classItem, int index)
    {
        var memberItem = classItem.Members[index];

        Assert.That(memberItem, Is.Not.Null);

        return memberItem;
    }
}
