using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public class EnumMapper
{
    public static CodeItem MapEnumMember(EnumMemberDeclarationSyntax member,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeItem>(member, semanticModel, codeDocumentViewModel, member.Identifier);
        codeItem.Kind = CodeItemKindEnum.EnumMember;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);

        return codeItem;
    }

    public static CodeClassItem MapEnum(EnumBlockSyntax member,
        SemanticModel semanticModel, SyntaxTree tree, CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeClassItem>(member, semanticModel, codeDocumentViewModel,
            member.EnumStatement.Identifier, modifiers: member.EnumStatement.Modifiers);
        codeItem.Kind = CodeItemKindEnum.Enum;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);
        codeItem.Parameters = MapMembersToString(member.Members);
        codeItem.Tooltip = TooltipMapper.Map(member, codeItem.Access, string.Empty, codeItem.Name, codeItem.Parameters);

        foreach (var enumMember in member.Members)
        {
            codeItem.Members.AddIfNotNull(DocumentMapper.MapMember(enumMember, tree, semanticModel, codeDocumentViewModel));
        }

        return codeItem;
    }

    private static string MapMembersToString(SyntaxList<StatementSyntax> members)
    {
        var enumMembers = members
            .Cast<EnumMemberDeclarationSyntax>()
            .Select(member => member.Identifier.Text);

        return string.Join(", ", enumMembers);
    }
}
