using CodeNav.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.Mappers
{
    public class IndexerMapper
    {
        public static CodeItem MapIndexer(IndexerDeclarationSyntax member, CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeFunctionItem>(member, member.ThisKeyword, member.Modifiers, control, semanticModel);
            item.Type = TypeMapper.Map(member.Type);
            item.Parameters = ParameterMapper.MapParameters(member.ParameterList);
            item.Tooltip = TooltipMapper.Map(item.Access, item.Type, item.Name, item.Parameters);
            item.Kind = CodeItemKindEnum.Indexer;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            return item;
        }
    }
}
