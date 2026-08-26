using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Windows;

namespace CodeNav.OutOfProc.Languages.CSharp.Mappers;

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

    public static CodeItem MapEnum(EnumDeclarationSyntax member,
        SemanticModel semanticModel, SyntaxTree tree, CodeDocumentViewModel codeDocumentViewModel)
    {
        CodeItem codeItem;

        var enumMembers = member
            .Members
            .Select(enumMember => DocumentMapper.MapMember(enumMember, tree, semanticModel, codeDocumentViewModel))
            .FilterNull()
            .ToList();

        VisibilityHelper.SetCodeItemVisibility(codeDocumentViewModel, enumMembers, codeDocumentViewModel.FilterRules);

        if (enumMembers.Any(enumMember => enumMember.Visibility == Visibility.Visible))
        {
            // Map enum as item containing members
            codeItem = BaseMapper.MapBase<CodeClassItem>(member, semanticModel, codeDocumentViewModel, member.Identifier, modifiers: member.Modifiers);
            
            ((CodeClassItem)codeItem).Members.AddRange(enumMembers);
            ((CodeClassItem)codeItem).Parameters = MapMembersToString(member.Members);
            ((CodeClassItem)codeItem).Tooltip = TooltipMapper.Map(member, codeItem.Access, string.Empty, codeItem.Name, ((CodeClassItem)codeItem).Parameters);
        }
        else
        {
            // Map enum as single item
            codeItem = BaseMapper.MapBase<CodeFunctionItem>(member, semanticModel, codeDocumentViewModel, member.Identifier, modifiers: member.Modifiers);

            ((CodeFunctionItem)codeItem).Parameters = MapMembersToString(member.Members);
            ((CodeFunctionItem)codeItem).Tooltip = TooltipMapper.Map(member, codeItem.Access, string.Empty, codeItem.Name, ((CodeFunctionItem)codeItem).Parameters);
        }

        codeItem.Kind = CodeItemKindEnum.Enum;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);

        return codeItem;
    }

    private static string MapMembersToString(SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
        => $"{string.Join(", ", members.Select(member => member.Identifier.Text))}";
}
