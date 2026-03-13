using CodeNav.Constants;
using CodeNav.Mappers;
using CodeNav.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.Languages.CSharp.Mappers;

public class IndexerMapper(BaseMapper baseMapper)
{
    public CodeItem MapIndexer(IndexerDeclarationSyntax member,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = baseMapper.MapBase<CodeFunctionItem>(member, member.ThisKeyword, member.Modifiers, semanticModel, codeDocumentViewModel);
        
        codeItem.ReturnType = TypeMapper.Map(member.Type);
        codeItem.Parameters = ParameterMapper.MapParameters(member.ParameterList);
        codeItem.Tooltip = TooltipMapper.Map(member, codeItem.Access, codeItem.ReturnType, codeItem.Name, codeItem.Parameters);
        codeItem.Kind = CodeItemKindEnum.Indexer;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);

        return codeItem;
    }
}
