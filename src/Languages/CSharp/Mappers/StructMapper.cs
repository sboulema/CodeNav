using CodeNav.Constants;
using CodeNav.Extensions;
using CodeNav.Mappers;
using CodeNav.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.Languages.CSharp.Mappers;

public static class StructMapper
{
    public static CodeClassItem MapStruct(StructDeclarationSyntax member,
        SemanticModel semanticModel, SyntaxTree tree, CodeDocumentViewModel codeDocumentViewModel)
    {
        var item = BaseMapper.MapBase<CodeClassItem>(member, member.Identifier, member.Modifiers, semanticModel, codeDocumentViewModel);
        item.Kind = CodeItemKindEnum.Struct;
        item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

        if (TriviaSummaryMapper.HasSummary(member))
        {
            item.Tooltip = TriviaSummaryMapper.Map(member);
        }

        foreach (var structMember in member.Members)
        {
            item.Members.AddIfNotNull(DocumentMapper.MapMember(structMember, tree, semanticModel, codeDocumentViewModel));
        }

        return item;
    }
}
