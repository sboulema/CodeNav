using CodeNav.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic;

namespace CodeNav.Mappers
{
    public class PropertyMapper
    {
        public static CodePropertyItem MapProperty(PropertyDeclarationSyntax member,
            CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodePropertyItem>(member, member.Identifier, member.Modifiers, control, semanticModel);
            item.Type = TypeMapper.Map(member.Type);

            if (member.AccessorList != null)
            {
                if (member.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.GetAccessorDeclaration))
                {
                    item.Parameters += "get";
                }

                if (member.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.SetAccessorDeclaration))
                {
                    item.Parameters += string.IsNullOrEmpty(item.Parameters) ? "set" : ",set";
                }

                if (!string.IsNullOrEmpty(item.Parameters))
                {
                    item.Parameters = $" {{{item.Parameters}}}";
                }
            }

            item.Tooltip = TooltipMapper.Map(item.Access, item.Type, item.Name, item.Parameters);
            item.Kind = CodeItemKindEnum.Property;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            return item;
        }

        public static CodePropertyItem MapProperty(VisualBasicSyntax.PropertyBlockSyntax member,
            CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodePropertyItem>(member, member.PropertyStatement.Identifier, 
                member.PropertyStatement.Modifiers, control, semanticModel);

            var symbol = semanticModel.GetDeclaredSymbol(member) as IPropertySymbol;
            item.Type = TypeMapper.Map(symbol.Type);

            if (member.Accessors != null)
            {
                if (member.Accessors.Any(a => a.Kind() == VisualBasic.SyntaxKind.GetAccessorBlock))
                {
                    item.Parameters += "get";
                }

                if (member.Accessors.Any(a => a.Kind() == VisualBasic.SyntaxKind.SetAccessorBlock))
                {
                    item.Parameters += string.IsNullOrEmpty(item.Parameters) ? "set" : ",set";
                }

                if (!string.IsNullOrEmpty(item.Parameters))
                {
                    item.Parameters = $" {{{item.Parameters}}}";
                }
            }

            item.Tooltip = TooltipMapper.Map(item.Access, item.Type, item.Name, item.Parameters);
            item.Kind = CodeItemKindEnum.Property;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            return item;
        }
    }
}
