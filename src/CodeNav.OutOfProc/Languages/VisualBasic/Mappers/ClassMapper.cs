using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class ClassMapper
{
    public static CodeClassItem MapClass(
        ClassBlockSyntax member,
        SemanticModel semanticModel,
        SyntaxTree tree,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeClassItem>(
            member,
            semanticModel,
            codeDocumentViewModel,
            member.ClassStatement.Identifier,
            modifiers: member.ClassStatement.Modifiers);

        codeItem.Kind = CodeItemKindEnum.Class;
        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);

        codeItem.Parameters = MapInheritance(member);

        codeItem.Tooltip = TooltipMapper.Map(
            member,
            codeItem.Access,
            string.Empty,
            codeItem.Name,
            codeItem.Parameters);

        var regions = RegionMapper.MapRegions(
            tree,
            member.Span,
            codeDocumentViewModel);

        foreach (var classMember in member.Members)
        {
            foreach (var memberItem in DocumentMapper.MapMembers(
                classMember,
                tree,
                semanticModel,
                codeDocumentViewModel))
            {
                if (RegionMapper.AddToRegion(regions, memberItem))
                {
                    continue;
                }

                codeItem.Members.Add(memberItem);
            }
        }

        RegionMapper.AddRegionsIfNotPresent(
            codeItem.Members,
            regions);

        return codeItem;
    }

    private static string MapInheritance(
        ClassBlockSyntax member)
    {
        var inheritance = new List<string>();

        foreach (var inheritsStatement in member.Inherits)
        {
            inheritance.AddRange(
                inheritsStatement.Types.Select(type => type.ToString()));
        }

        foreach (var implementsStatement in member.Implements)
        {
            inheritance.AddRange(
                implementsStatement.Types.Select(
                    interfaceType => interfaceType.ToString()));
        }

        return inheritance.Any()
            ? $" : {string.Join(", ", inheritance)}"
            : string.Empty;
    }
}
