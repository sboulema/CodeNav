using CodeNav.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic;

namespace CodeNav.Mappers
{
    public static class FieldMapper
    {
        public static CodeItem MapField(FieldDeclarationSyntax member, 
            CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeItem>(member, member.Declaration.Variables.First().Identifier, member.Modifiers, control, semanticModel);
            item.Kind = IsConstant(member.Modifiers)
                ? CodeItemKindEnum.Constant
                : CodeItemKindEnum.Variable;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            return item;
        }

        public static CodeItem MapField(VisualBasicSyntax.FieldDeclarationSyntax member,
            CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeItem>(member, member.Declarators.First().Names.First().Identifier, member.Modifiers, control, semanticModel);
            item.Kind = IsConstant(member.Modifiers)
                ? CodeItemKindEnum.Constant
                : CodeItemKindEnum.Variable;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            return item;
        }

        private static bool IsConstant(SyntaxTokenList modifiers)
        {
            if (modifiers.First().Language == "Visual Basic")
            {
                return modifiers.Any(m => m.RawKind == (int)VisualBasic.SyntaxKind.ConstKeyword);
            }
            else
            {
                return modifiers.Any(m => m.Kind() == SyntaxKind.ConstKeyword);
            }           
        }
    }
}
