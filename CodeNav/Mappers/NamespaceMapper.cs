using CodeNav.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.Mappers
{
    public static class NamespaceMapper
    {
        public static CodeNamespaceItem MapNamespace(NamespaceDeclarationSyntax member, 
            CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeNamespaceItem>(member, member.Name, control, semanticModel);
            item.Kind = CodeItemKindEnum.Namespace;
            foreach (var namespaceMember in member.Members)
            {
                item.Members.Add(SyntaxMapper.MapMember(namespaceMember));
            }
            return item;
        }

        public static CodeNamespaceItem MapNamespace(VisualBasicSyntax.NamespaceBlockSyntax member, 
            CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeNamespaceItem>(member, member.NamespaceStatement.Name, control, semanticModel);
            item.Kind = CodeItemKindEnum.Namespace;
            foreach (var namespaceMember in member.Members)
            {
                item.Members.Add(SyntaxMapper.MapMember(namespaceMember));
            }
            return item;
        }
    }
}
