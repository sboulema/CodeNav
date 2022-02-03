using CodeNav.Helpers;
using CodeNav.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Windows.Media;

namespace CodeNav.Mappers
{
    public static class RecordMapper
    {
        public static CodeClassItem MapRecord(TypeDeclarationSyntax member,
            ICodeViewUserControl control, SemanticModel semanticModel, SyntaxTree tree)
        {
            if (member == null)
            {
                return null;
            }

            var item = BaseMapper.MapBase<CodeClassItem>(member, member.Identifier, member.Modifiers, control, semanticModel);
            item.Kind = CodeItemKindEnum.Record;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.BorderColor = Colors.DarkGray;

            if (TriviaSummaryMapper.HasSummary(member) && SettingsHelper.UseXMLComments)
            {
                item.Tooltip = TriviaSummaryMapper.Map(member);
            }

            foreach (var recordMember in member.Members)
            {
                item.Members.Add(SyntaxMapper.MapMember(recordMember, tree, semanticModel, control));
            }

            return item;
        }
    }
}
