using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public class ClassMapper
{
    public static CodeClassItem MapClass(TypeBlockSyntax member,
        SemanticModel semanticModel, SyntaxTree tree, CodeDocumentViewModel codeDocumentViewModel,
        bool mapBaseClass)
    {
        var codeItem = BaseMapper.MapBase<CodeClassItem>(member, semanticModel, codeDocumentViewModel,
            member.BlockStatement.Identifier, modifiers: member.BlockStatement.Modifiers);
        codeItem.Kind = CodeItemKindEnum.Class;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);
        codeItem.Parameters = MapInheritance(member);
        codeItem.Tooltip = TooltipMapper.Map(member, codeItem.Access, string.Empty, codeItem.Name, codeItem.Parameters);

        // Map implemented interfaces
        var implementedInterfaces = InterfaceMapper.MapImplementedInterfaces(member, semanticModel, tree, codeDocumentViewModel);

        // Map regions
        var regions = RegionMapper.MapRegions(tree, member.Span, codeDocumentViewModel);

        // Map members from the base class
        if (mapBaseClass)
        {
            MapMembersFromBaseClass(member, regions, semanticModel, codeDocumentViewModel);
        }

        // Map class members
        foreach (var classMember in member.Members)
        {
            var memberItem = DocumentMapper.MapMember(classMember, tree, semanticModel, codeDocumentViewModel);

            if (memberItem == null)
            {
                continue;
            }

            // Skip member if it is part of an implemented interface
            if (InterfaceMapper.IsPartOfImplementedInterface(implementedInterfaces, memberItem))
            {
                continue;
            }

            // Add member to region if it is part of one
            if (RegionMapper.AddToRegion(regions, memberItem))
            {
                continue;
            }

            // Still here? Add the member to the class
            codeItem.Members.Add(memberItem);
        }

        // Add implemented interfaces to class or region if they have a interface member inside them
        foreach (var implementedInterfaceItem in implementedInterfaces
            .Where(implementedInterface => implementedInterface.Members.Any()))
        {
            foreach (var implementedInterfaceItemMember in implementedInterfaceItem.Members)
            {
                // Check if the implemented interfaces is part of a region or not
                if (RegionMapper.IsPartOfRegion(regions, implementedInterfaceItemMember))
                {
                    // If it is part of a region, add the implemented interface item to the region, add the member to the interface item
                    var regionItem = RegionMapper.GetRegion(regions, implementedInterfaceItemMember);
                    AddImplementedInterfaceMember(regionItem!, implementedInterfaceItem, implementedInterfaceItemMember);
                }
                else
                {
                    // If it is not part of a region, add the implemented interface item tot the class, add the member to the interface item 
                    AddImplementedInterfaceMember(codeItem, implementedInterfaceItem, implementedInterfaceItemMember);
                }
            }
        }

        // Add regions to class
        codeItem.Members.AddRange(regions);

        return codeItem;
    }

    /// <summary>
    /// Check if the containing code item already has a Implemented interface item
    /// if not: Create an empty implemented interface item
    /// Add code item to implemented interface item
    /// </summary>
    /// <param name="membersItem"></param>
    /// <param name="implementedInterfaceItem"></param>
    /// <param name="codeItem"></param>
    private static void AddImplementedInterfaceMember(
        IMembers membersItem,
        CodeImplementedInterfaceItem implementedInterfaceItem,
        CodeItem codeItem)
    {
        if (!membersItem.Members.Any(memberItem => memberItem.Id == implementedInterfaceItem.Id))
        {
            var implementedInterfaceStub = implementedInterfaceItem.Clone() as CodeImplementedInterfaceItem;
            implementedInterfaceStub!.Members = [];

            membersItem.Members.Add(implementedInterfaceStub);
        }

        var implementedInterfaceItemMember = membersItem
            .Members
            .First(memberItem => memberItem.Id == implementedInterfaceItem.Id) as CodeImplementedInterfaceItem;

        implementedInterfaceItemMember!.Members.Add(codeItem);
    }

    private static string MapInheritance(TypeBlockSyntax member)
    {
        if (member?.Inherits == null)
        {
            return string.Empty;
        }

        var inheritanceList = member
            .Inherits
            .SelectMany(inherit => inherit.Types.Select(type => type.ToString()));

        return !inheritanceList.Any() ? string.Empty : $" : {string.Join(", ", inheritanceList)}";
    }

    /// <summary>
    /// Map all members from the base class and add them to a region in the derived class.
    /// </summary>
    /// <param name="member"></param>
    /// <param name="regions"></param>
    /// <param name="semanticModel"></param>
    /// <param name="codeDocumentViewModel"></param>
    private static void MapMembersFromBaseClass(TypeBlockSyntax member,
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
