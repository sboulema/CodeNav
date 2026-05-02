using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public class PropertyMapper
{
    public static CodePropertyItem MapProperty(PropertyBlockSyntax member,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodePropertyItem>(
            member, semanticModel, codeDocumentViewModel,
            member.PropertyStatement.Identifier, modifiers: member.PropertyStatement.Modifiers);

        codeItem.IdentifierSpan = member.PropertyStatement.Identifier.Span;

        // https://github.com/dotnet/roslyn-analyzers/issues/7436
        var symbol = semanticModel.GetDeclaredSymbol(member.PropertyStatement);

        codeItem.ReturnType = TypeMapper.Map(symbol?.Type);

        if (member.Accessors != null)
        {
            if (member.Accessors.Any(a => a.Kind() == SyntaxKind.GetAccessorBlock))
            {
                codeItem.Parameters += "get";
            }

            if (member.Accessors.Any(a => a.Kind() == SyntaxKind.SetAccessorBlock))
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
