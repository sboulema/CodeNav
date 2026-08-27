using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.Languages.TypeScript.Parsing;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using System.Windows;

namespace CodeNav.OutOfProc.Languages.TypeScript.Mappers;

public static class CodeItemMapper
{
    public static List<CodeItem> MapNodes(
        IEnumerable<TypeScriptNode> nodes,
        CodeDocumentViewModel codeDocumentViewModel,
        string parentFullName,
        bool isMember,
        CodeItemKindEnum? parentKind = null)
        => [.. nodes
            .Select(node => MapNode(node, codeDocumentViewModel, parentFullName, isMember, parentKind))
            .Where(codeItem => codeItem != null)
            .Cast<CodeItem>()];

    public static CodeItem? MapNode(
        TypeScriptNode node,
        CodeDocumentViewModel codeDocumentViewModel,
        string parentFullName,
        bool isMember,
        CodeItemKindEnum? parentKind = null)
        => node.Kind switch
        {
            CodeItemKindEnum.Namespace => MapContainer<CodeNamespaceItem>(node, codeDocumentViewModel, parentFullName),
            CodeItemKindEnum.Class => MapContainer<CodeClassItem>(node, codeDocumentViewModel, parentFullName, includeHeritage: true),
            CodeItemKindEnum.Interface => MapContainer<CodeInterfaceItem>(node, codeDocumentViewModel, parentFullName, includeHeritage: true),
            CodeItemKindEnum.Region => MapRegion(node, codeDocumentViewModel, parentFullName, isMember, parentKind),
            CodeItemKindEnum.Enum => MapEnum(node, codeDocumentViewModel, parentFullName),
            CodeItemKindEnum.EnumMember => MapEnumMember(node, codeDocumentViewModel, parentFullName),
            CodeItemKindEnum.Constructor => MapFunction(node, codeDocumentViewModel, parentFullName, CodeItemKindEnum.Constructor, isMember, parentKind),
            CodeItemKindEnum.Method => MapFunction(node, codeDocumentViewModel, parentFullName, CodeItemKindEnum.Method, isMember, parentKind),
            CodeItemKindEnum.Property => MapProperty(node, codeDocumentViewModel, parentFullName, parentKind),
            CodeItemKindEnum.Variable or CodeItemKindEnum.Constant => MapVariable(node, codeDocumentViewModel, parentFullName),
            _ => null,
        };

    private static T MapContainer<T>(
        TypeScriptNode node,
        CodeDocumentViewModel codeDocumentViewModel,
        string parentFullName,
        bool includeHeritage = false) where T : CodeItem, IMembers, new()
    {
        CodeItem codeItem;

        var fullName = string.IsNullOrEmpty(parentFullName)
            ? node.Name
            : $"{parentFullName}.{node.Name}";
        var isChildMember = node.Kind is CodeItemKindEnum.Class or CodeItemKindEnum.Interface;
        var members = MapNodes(node.Members, codeDocumentViewModel, fullName, isChildMember, node.Kind);

        if (node.Kind is CodeItemKindEnum.Interface)
        {
            members
                .ForEach(interfaceMember => interfaceMember.AdditionalKinds.Add(CodeItemKindEnum.InterfaceMember));
        }

        VisibilityHelper.SetCodeItemVisibility(codeDocumentViewModel, members, codeDocumentViewModel.FilterRules);

        if (members.Any(enumMember => enumMember.Visibility == Visibility.Visible))
        {
            codeItem = BaseMapper.MapBase<T>(node, codeDocumentViewModel, parentFullName, isMember: false);

            ((T)codeItem).Members.AddRange(members);

            if (codeItem is CodeClassItem classItem)
            {
                classItem.Parameters = includeHeritage ? node.Heritage : string.Empty;
            }
        }
        else
        {
            codeItem = BaseMapper.MapBase<CodePropertyItem>(node, codeDocumentViewModel, parentFullName, isMember: false);
        }

        codeItem.Kind = node.Kind;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);
        codeItem.Tooltip = TooltipMapper.Map(codeItem.Access, string.Empty, codeItem.Name,
            codeItem is CodeClassItem heritageItem ? heritageItem.Parameters : string.Empty);

        return (T)codeItem;
    }

    private static CodeRegionItem MapRegion(
        TypeScriptNode node,
        CodeDocumentViewModel codeDocumentViewModel,
        string parentFullName,
        bool isMember,
        CodeItemKindEnum? parentKind)
    {
        // Regions are transparent with respect to the qualified name and member-ness of their
        // contents: a region inside a class still contains class members, and one inside a
        // namespace still contains namespace members.
        var codeItem = new CodeRegionItem
        {
            Name = node.Name,
            FullName = node.Name,
            Id = node.Name,
            Tooltip = node.Name,
            Kind = CodeItemKindEnum.Region,
            Span = node.Span,
            IdentifierSpan = node.IdentifierSpan,
            OutlineSpan = node.Span,
            Moniker = IconMapper.MapMoniker(CodeItemKindEnum.Region, CodeItemAccessEnum.Unknown),
            CodeDocumentViewModel = codeDocumentViewModel,
        };

        codeItem.Members = MapNodes(node.Members, codeDocumentViewModel, parentFullName, isMember, parentKind);

        return codeItem;
    }

    private static CodeItem MapEnum(TypeScriptNode node, CodeDocumentViewModel codeDocumentViewModel, string parentFullName)
    {
        CodeItem codeItem;

        var fullName = string.IsNullOrEmpty(parentFullName)
            ? node.Name
            : $"{parentFullName}.{node.Name}";

        var enumMembers = MapNodes(node.Members, codeDocumentViewModel, fullName, isMember: false);

        VisibilityHelper.SetCodeItemVisibility(codeDocumentViewModel, enumMembers, codeDocumentViewModel.FilterRules);

        if (enumMembers.Any(enumMember => enumMember.Visibility == Visibility.Visible))
        {
            // Map enum as item containing members
            codeItem = BaseMapper.MapBase<CodeClassItem>(node, codeDocumentViewModel, parentFullName, isMember: false);

            ((CodeClassItem)codeItem).Parameters = node.Type;
            codeItem.Tooltip = TooltipMapper.Map(codeItem.Access, string.Empty, codeItem.Name, ((CodeClassItem)codeItem).Parameters);
            ((CodeClassItem)codeItem).Members.AddRange(enumMembers);
        }
        else
        {
            // Map enum as single item
            codeItem = BaseMapper.MapBase<CodeFunctionItem>(node, codeDocumentViewModel, parentFullName, isMember: false);

            ((CodeFunctionItem)codeItem).Parameters = node.Type;
            codeItem.Tooltip = TooltipMapper.Map(codeItem.Access, string.Empty, codeItem.Name, ((CodeFunctionItem)codeItem).Parameters);
        }

        codeItem.Kind = CodeItemKindEnum.Enum;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);

        return codeItem;
    }

    private static CodeItem MapEnumMember(TypeScriptNode node, CodeDocumentViewModel codeDocumentViewModel, string parentFullName)
    {
        var codeItem = BaseMapper.MapBase<CodeItem>(node, codeDocumentViewModel, parentFullName, isMember: false);
        codeItem.Kind = CodeItemKindEnum.EnumMember;
        codeItem.Access = CodeItemAccessEnum.Public;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);

        return codeItem;
    }

    private static CodeFunctionItem MapFunction(
        TypeScriptNode node,
        CodeDocumentViewModel codeDocumentViewModel,
        string parentFullName,
        CodeItemKindEnum kind,
        bool isMember,
        CodeItemKindEnum? parentKind)
    {
        var codeItem = BaseMapper.MapBase<CodeFunctionItem>(node, codeDocumentViewModel, parentFullName, isMember);
        codeItem.Kind = kind;
        codeItem.Parameters = node.Parameters;
        codeItem.ReturnType = node.Type;

        if (parentKind == CodeItemKindEnum.Interface)
        {
            codeItem.AdditionalKinds.Add(CodeItemKindEnum.InterfaceMember);
        }

        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);
        codeItem.Tooltip = TooltipMapper.Map(codeItem.Access, codeItem.ReturnType, codeItem.Name, codeItem.Parameters);

        return codeItem;
    }

    private static CodePropertyItem MapProperty(TypeScriptNode node, CodeDocumentViewModel codeDocumentViewModel, string parentFullName, CodeItemKindEnum? parentKind)
    {
        var codeItem = BaseMapper.MapBase<CodePropertyItem>(node, codeDocumentViewModel, parentFullName, isMember: true);
        codeItem.Kind = CodeItemKindEnum.Property;
        codeItem.ReturnType = node.Type;

        if (parentKind == CodeItemKindEnum.Interface)
        {
            codeItem.AdditionalKinds.Add(CodeItemKindEnum.InterfaceMember);
        }

        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);
        codeItem.Tooltip = TooltipMapper.Map(codeItem.Access, codeItem.ReturnType, codeItem.Name, string.Empty);

        return codeItem;
    }

    private static CodeItem MapVariable(TypeScriptNode node, CodeDocumentViewModel codeDocumentViewModel, string parentFullName)
    {
        var codeItem = BaseMapper.MapBase<CodeItem>(node, codeDocumentViewModel, parentFullName, isMember: false);
        codeItem.Kind = node.Kind;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);
        codeItem.Tooltip = TooltipMapper.Map(codeItem.Access, node.Type, codeItem.Name, string.Empty);

        return codeItem;
    }
}