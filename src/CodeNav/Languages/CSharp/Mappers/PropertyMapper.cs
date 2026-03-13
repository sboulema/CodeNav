using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeNav.ViewModels;
using CodeNav.Constants;
using CodeNav.Mappers;

namespace CodeNav.Languages.CSharp.Mappers;

public class PropertyMapper(BaseMapper baseMapper)
{
    public CodePropertyItem MapProperty(PropertyDeclarationSyntax member,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = baseMapper.MapBase<CodePropertyItem>(member, member.Identifier, member.Modifiers, semanticModel, codeDocumentViewModel);
        codeItem.IdentifierSpan = member.Identifier.Span;
        codeItem.ReturnType = TypeMapper.Map(member.Type);

        if (member.AccessorList != null)
        {
            if (member.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.GetAccessorDeclaration))
            {
                codeItem.Parameters += "get";
            }

            if (member.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.SetAccessorDeclaration))
            {
                codeItem.Parameters += string.IsNullOrEmpty(codeItem.Parameters) ? "set" : ",set";
            }

            if (!string.IsNullOrEmpty(codeItem.Parameters))
            {
                codeItem.Parameters = $" {{{codeItem.Parameters}}}";
            }
        }

        codeItem.Tooltip = TooltipMapper.Map(member, codeItem.Access, codeItem.ReturnType, codeItem.Name, codeItem.Parameters);
        codeItem.Kind = CodeItemKindEnum.Property;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);

        return codeItem;
    }
}
