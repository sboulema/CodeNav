using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class DelegateEventMapper
{
    public static CodeItem MapDelegate(DelegateStatementSyntax member, SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var item = BaseMapper.MapBase<CodeItem>(member, semanticModel, codeDocumentViewModel, member.Identifier, modifiers: member.Modifiers);
        item.Kind = CodeItemKindEnum.Delegate;
        item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
        return item;
    }

    public static CodeItem MapEvent(EventBlockSyntax member, SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var item = BaseMapper.MapBase<CodeItem>(member, semanticModel, codeDocumentViewModel, member.EventStatement.Identifier,
            modifiers: member.EventStatement.Modifiers);
        item.Kind = CodeItemKindEnum.Event;
        item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
        return item;
    }
}
