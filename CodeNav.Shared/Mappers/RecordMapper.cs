using CodeNav.Helpers;
using CodeNav.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.Mappers
{
    public static class RecordMapper
    {
        #if VS2022
        public static CodeFunctionItem MapRecord(RecordDeclarationSyntax member,
            ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null)
            {
                return null;
            }

            var item = BaseMapper.MapBase<CodeFunctionItem>(member, member.Identifier, member.Modifiers, control, semanticModel);
            item.Kind = CodeItemKindEnum.Record;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.Parameters = ParameterMapper.MapParameters(member.ParameterList);

            if (TriviaSummaryMapper.HasSummary(member) && SettingsHelper.UseXMLComments)
            {
                item.Tooltip = TriviaSummaryMapper.Map(member);
            }

            return item;
        }
        #endif
    }
}
