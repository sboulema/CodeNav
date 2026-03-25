using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.OutOfProc.Languages.CSharp.Mappers;

public class IndexerMapper
{
    public static CodeItem MapIndexer(IndexerDeclarationSyntax member,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeFunctionItem>(member, semanticModel, codeDocumentViewModel, member.ThisKeyword, modifiers: member.Modifiers);
        
        codeItem.ReturnType = TypeMapper.Map(member.Type);
        codeItem.Parameters = ParameterMapper.MapParameters(member.ParameterList);
        codeItem.Tooltip = TooltipMapper.Map(member, codeItem.Access, codeItem.ReturnType, codeItem.Name, codeItem.Parameters);
        codeItem.Kind = CodeItemKindEnum.Indexer;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);

        return codeItem;
    }
}
