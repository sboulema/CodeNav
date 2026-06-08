using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Extensibility;

namespace CodeNav.OutOfProc.Languages.CSharp.Mappers;

public class ExtensionBlockMapper
{
    public static CodeClassItem MapExtensionBlock(ExtensionBlockDeclarationSyntax member,
        SemanticModel semanticModel, SyntaxTree tree, CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeClassItem>(member, semanticModel, codeDocumentViewModel,
            member.Identifier, modifiers: member.Modifiers, name: "Extension");
        codeItem.Kind = CodeItemKindEnum.ExtensionBlock;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);
        codeItem.OverlayMoniker = ImageMoniker.KnownValues.OverlayLoginDisabled;
        codeItem.Parameters = ParameterMapper.MapParameters(member.ParameterList);
        codeItem.Tooltip = TooltipMapper.Map(member, codeItem.Access, string.Empty, codeItem.Name, codeItem.Parameters);

        // Map regions
        var regions = RegionMapper.MapRegions(tree, member.Span, codeDocumentViewModel);

        // Map block members
        foreach (var blockMember in member.Members)
        {
            var memberItem = DocumentMapper.MapMember(blockMember, tree, semanticModel, codeDocumentViewModel);

            if (memberItem == null)
            {
                continue;
            }

            // Add member to region if it is part of one
            if (RegionMapper.AddToRegion(regions, memberItem))
            {
                continue;
            }

            // Still here? Add the member to the block
            codeItem.Members.Add(memberItem);
        }

        // Add regions to block
        codeItem.Members.AddRange(regions);

        return codeItem;
    }
}
