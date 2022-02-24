#nullable enable

using CodeNav.Extensions;
using CodeNav.Helpers;
using CodeNav.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Windows.Media;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.Mappers
{
    public static class EnumMapper
    {
        public static CodeItem? MapEnumMember(EnumMemberDeclarationSyntax? member,
            ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null)
            {
                return null;
            }

            var item = BaseMapper.MapBase<CodeItem>(member, member.Identifier, control, semanticModel);
            item.Kind = CodeItemKindEnum.EnumMember;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            return item;
        }

        public static CodeItem? MapEnumMember(VisualBasicSyntax.EnumMemberDeclarationSyntax? member,
            ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null)
            {
                return null;
            }

            var item = BaseMapper.MapBase<CodeItem>(member, member.Identifier, control, semanticModel);
            item.Kind = CodeItemKindEnum.EnumMember;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            return item;
        }

        public static CodeClassItem? MapEnum(EnumDeclarationSyntax? member,
            ICodeViewUserControl control, SemanticModel semanticModel, SyntaxTree tree)
        {
            if (member == null)
            {
                return null;
            }

            var item = BaseMapper.MapBase<CodeClassItem>(member, member.Identifier, member.Modifiers, control, semanticModel);
            item.Kind = CodeItemKindEnum.Enum;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.Parameters = MapMembersToString(member.Members);
            item.BorderColor = Colors.DarkGray;

            if (TriviaSummaryMapper.HasSummary(member) && SettingsHelper.UseXMLComments)
            {
                item.Tooltip = TriviaSummaryMapper.Map(member);
            }

            foreach (var enumMember in member.Members)
            {
                item.Members.AddIfNotNull(SyntaxMapper.MapMember(enumMember, tree, semanticModel, control));
            }

            return item;
        }

        public static CodeClassItem? MapEnum(VisualBasicSyntax.EnumBlockSyntax? member,
            ICodeViewUserControl control, SemanticModel semanticModel, SyntaxTree tree)
        {
            if (member == null)
            {
                return null;
            }

            var item = BaseMapper.MapBase<CodeClassItem>(member, member.EnumStatement.Identifier, 
                member.EnumStatement.Modifiers, control, semanticModel);
            item.Kind = CodeItemKindEnum.Enum;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.Parameters = MapMembersToString(member.Members);
            item.BorderColor = Colors.DarkGray;

            if (TriviaSummaryMapper.HasSummary(member) && SettingsHelper.UseXMLComments)
            {
                item.Tooltip = TriviaSummaryMapper.Map(member);
            }

            foreach (var enumMember in member.Members)
            {
                item.Members.AddIfNotNull(SyntaxMapper.MapMember(enumMember, tree, semanticModel, control));
            }

            return item;
        }

        private static string MapMembersToString(SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
        {
            var memberList = (from EnumMemberDeclarationSyntax member in members select member.Identifier.Text).ToList();
            return $"{string.Join(", ", memberList)}";
        }

        private static string MapMembersToString(SyntaxList<VisualBasicSyntax.StatementSyntax> members)
        {
            var memberList = (from VisualBasicSyntax.EnumMemberDeclarationSyntax member in members select member.Identifier.Text).ToList();
            return $"{string.Join(", ", memberList)}";
        }
    }
}
