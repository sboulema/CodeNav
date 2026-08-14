using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class ModuleMapper
{
    public static CodeItem MapModule(
        ModuleBlockSyntax member,
        SemanticModel semanticModel,
        SyntaxTree tree,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var statement = member.ModuleStatement;

        var codeItem = BaseMapper.MapBase<CodeClassItem>(
            member,
            semanticModel,
            codeDocumentViewModel,
            statement.Identifier,
            modifiers: statement.Modifiers);

        codeItem.Kind = CodeItemKindEnum.Class;
        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);

        codeItem.Tooltip = TooltipMapper.Map(
            member,
            codeItem.Access,
            string.Empty,
            codeItem.Name,
            string.Empty);

        var regions = RegionMapper.MapRegions(
            tree,
            member.Span,
            codeDocumentViewModel);

        foreach (var moduleMember in member.Members)
        {
            foreach (var memberItem in DocumentMapper.MapMembers(
                moduleMember,
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
}
