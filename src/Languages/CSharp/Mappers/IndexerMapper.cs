using CodeNav.Constants;
using CodeNav.Mappers;
using CodeNav.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.Languages.CSharp.Mappers;

public class IndexerMapper
{
    public static CodeItem MapIndexer(IndexerDeclarationSyntax member,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var item = BaseMapper.MapBase<CodeFunctionItem>(member, member.ThisKeyword, member.Modifiers, semanticModel, codeDocumentViewModel);
        item.ReturnType = TypeMapper.Map(member.Type);
        item.Parameters = ParameterMapper.MapParameters(member.ParameterList);
        item.Tooltip = TooltipMapper.Map(item.Access, item.ReturnType, item.Name, item.Parameters);
        item.Kind = CodeItemKindEnum.Indexer;
        item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

        if (TriviaSummaryMapper.HasSummary(member))
        {
            item.Tooltip = TriviaSummaryMapper.Map(member);
        }

        return item;
    }
}
