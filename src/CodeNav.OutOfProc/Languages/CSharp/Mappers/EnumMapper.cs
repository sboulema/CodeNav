using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.OutOfProc.Languages.CSharp.Mappers;

public class EnumMapper
{
    public static CodeItem MapEnumMember(EnumMemberDeclarationSyntax member,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var item = BaseMapper.MapBase<CodeItem>(member, semanticModel, codeDocumentViewModel, member.Identifier);
        item.Kind = CodeItemKindEnum.EnumMember;
        item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

        return item;
    }

    public static CodeClassItem MapEnum(EnumDeclarationSyntax member,
        SemanticModel semanticModel, SyntaxTree tree, CodeDocumentViewModel codeDocumentViewModel)
    {
        var item = BaseMapper.MapBase<CodeClassItem>(member, semanticModel, codeDocumentViewModel, member.Identifier, modifiers: member.Modifiers);
        item.Kind = CodeItemKindEnum.Enum;
        item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
        item.Parameters = MapMembersToString(member.Members);

        if (TriviaSummaryMapper.HasSummary(member))
        {
            item.Tooltip = TriviaSummaryMapper.Map(member);
        }

        foreach (var enumMember in member.Members)
        {
            item.Members.AddIfNotNull(DocumentMapper.MapMember(enumMember, tree, semanticModel, codeDocumentViewModel));
        }

        return item;
    }

    private static string MapMembersToString(SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
        => $"{string.Join(", ", members.Select(member => member.Identifier.Text))}";
}
