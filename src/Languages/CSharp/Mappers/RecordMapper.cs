using CodeNav.Constants;
using CodeNav.Mappers;
using CodeNav.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.Languages.CSharp.Mappers;

public static class RecordMapper
{
    public static CodeFunctionItem? MapRecord(RecordDeclarationSyntax member,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var item = BaseMapper.MapBase<CodeFunctionItem>(member, member.Identifier, member.Modifiers, semanticModel, codeDocumentViewModel);
        item.Kind = CodeItemKindEnum.Record;
        item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
        item.Parameters = ParameterMapper.MapParameters(member.ParameterList);

        if (TriviaSummaryMapper.HasSummary(member))
        {
            item.Tooltip = TriviaSummaryMapper.Map(member);
        }

        return item;
    }
}
