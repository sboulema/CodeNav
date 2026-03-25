using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.OutOfProc.Languages.CSharp.Mappers;

public class ClassMapper
{
    public static CodeClassItem MapClass(ClassDeclarationSyntax member,
        SemanticModel semanticModel, SyntaxTree tree, CodeDocumentViewModel codeDocumentViewModel,
        bool mapBaseClass)
    {
        var codeItem = BaseMapper.MapBase<CodeClassItem>(member, semanticModel, codeDocumentViewModel, member.Identifier, modifiers: member.Modifiers);
        codeItem.Kind = CodeItemKindEnum.Class;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);
        codeItem.Parameters = MapInheritance(member);
        codeItem.Tooltip = TooltipMapper.Map(member, codeItem.Access, string.Empty, codeItem.Name, codeItem.Parameters);

        var regions = RegionMapper.MapRegions(tree, member.Span, codeDocumentViewModel);
        var implementedInterfaces = InterfaceMapper.MapImplementedInterfaces(member, semanticModel, tree, codeDocumentViewModel);

        // Map members from the base class
        if (mapBaseClass)
        {
            MapMembersFromBaseClass(member, regions, semanticModel, codeDocumentViewModel);
        }

        // Map class members
        foreach (var classMember in member.Members)
        {
            var memberItem = DocumentMapper.MapMember(classMember, tree, semanticModel, codeDocumentViewModel);
            if (memberItem != null && !InterfaceMapper.IsPartOfImplementedInterface(implementedInterfaces, memberItem)
                && !RegionMapper.AddToRegion(regions, memberItem))
            {
                codeItem.Members.Add(memberItem);
            }
        }

        // Add implemented interfaces to class or region if they have a interface member inside them
        if (implementedInterfaces.Any())
        {
            foreach (var interfaceItem in implementedInterfaces)
            {
                if (interfaceItem.Members.Any())
                {
                    if (!RegionMapper.AddToRegion(regions, interfaceItem))
                    {
                        codeItem.Members.Add(interfaceItem);
                    }
                }
            }
        }

        // Add regions to class
        if (regions.Any())
        {
            codeItem.Members.AddRange(regions);
        }

        return codeItem;
    }

    private static string MapInheritance(ClassDeclarationSyntax member)
    {
        if (member?.BaseList == null)
        {
            return string.Empty;
        }

        var inheritanceList = (from BaseTypeSyntax bases in member.BaseList.Types select bases.Type.ToString()).ToList();

        return !inheritanceList.Any() ? string.Empty : $" : {string.Join(", ", inheritanceList)}";
    }

    /// <summary>
    /// Map all members from the base class and add them to a region in the derived class.
    /// </summary>
    /// <param name="member"></param>
    /// <param name="regions"></param>
    /// <param name="semanticModel"></param>
    /// <param name="codeDocumentViewModel"></param>
    private static void MapMembersFromBaseClass(ClassDeclarationSyntax member,
        List<CodeRegionItem> regions, SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var classSymbol = semanticModel.GetDeclaredSymbol(member);
        var baseType = classSymbol?.BaseType;

        if (baseType == null ||
            baseType.SpecialType == SpecialType.System_Object)
        {
            return;
        }

        // Add region to derived class to group inherited members
        var baseRegion = new CodeRegionItem
        {
            Name = baseType.Name,
            FullName = baseType.Name,
            Id = baseType.Name,
            Tooltip = baseType.Name,
            Kind = CodeItemKindEnum.BaseClass,
            Moniker = IconMapper.MapMoniker(CodeItemKindEnum.BaseClass, CodeItemAccessEnum.Unknown),
        };

        regions.Add(baseRegion);

        // Get the syntax tree of the base class to retrieve its members
        var baseSyntaxTree = baseType.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree;

        if (baseSyntaxTree == null)
        {
            return;
        }

        var baseTypeMembers = baseType?.GetMembers();

        if (baseTypeMembers == null)
        {
            return;
        }

        // Add inherited members to the region in the derived class
        foreach (var inheritedMember in baseTypeMembers)
        {
            var syntaxNode = inheritedMember.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

            if (syntaxNode.IsKind(SyntaxKind.VariableDeclarator))
            {
                syntaxNode = syntaxNode?.Parent?.Parent;
            }

            if (syntaxNode == null)
            {
                continue;
            }

            var memberItem = DocumentMapper.MapMember(syntaxNode, syntaxNode.SyntaxTree,
                semanticModel, codeDocumentViewModel, mapBaseClass: false);

            baseRegion.Members.AddIfNotNull(memberItem);
        }
    }

}
