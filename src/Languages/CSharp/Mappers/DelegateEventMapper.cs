using CodeNav.Constants;
using CodeNav.Mappers;
using CodeNav.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.Languages.CSharp.Mappers;

public static class DelegateEventMapper
{
    public static CodeItem MapDelegate(DelegateDeclarationSyntax member, SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var item = BaseMapper.MapBase<CodeItem>(member, member.Identifier, member.Modifiers, semanticModel, codeDocumentViewModel);
        item.Kind = CodeItemKindEnum.Delegate;
        item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
        return item;
    }

    public static CodeItem MapEvent(EventFieldDeclarationSyntax member, SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var item = BaseMapper.MapBase<CodeItem>(member, member.Declaration.Variables.First().Identifier,
            member.Modifiers, semanticModel, codeDocumentViewModel);
        item.Kind = CodeItemKindEnum.Event;
        item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
        return item;
    }
}
