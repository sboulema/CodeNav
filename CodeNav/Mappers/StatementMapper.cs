using System.Windows.Media;
using CodeNav.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeNav.Helpers;

namespace CodeNav.Mappers
{
    public static class StatementMapper
    {
        public static CodeItem MapStatement(StatementSyntax statement)
        {
            if (statement == null) return null;

            switch (statement.Kind())
            {
                case SyntaxKind.SwitchStatement:
                    return MapSwitch(statement as SwitchStatementSyntax);
                default:
                    return null;
            }
        }

        private static CodeItem MapSwitch(SwitchStatementSyntax statement)
        {
            if (statement == null) return null;

            var item = SyntaxMapper.MapBase<CodeClassItem>(statement, statement.Expression.ToString());
            item.Name = $"Switch {item.Name}";
            item.Kind = CodeItemKindEnum.Switch;
            item.Moniker = SyntaxMapper.MapMoniker(item.Kind, item.Access);
            item.BorderBrush = ColorHelper.CreateSolidColorBrush(Colors.DarkGray);
            item.Tooltip = TooltipMapper.Map(item.Access, string.Empty, item.Name, item.Parameters);

            // Map switch cases
            foreach (var section in statement.Sections)
            {
                item.Members.Add(MapSwitchSection(section));
            }

            return item;
        }

        private static CodeItem MapSwitchSection(SwitchSectionSyntax section)
        {
            if (section == null) return null;

            var item = SyntaxMapper.MapBase<CodePropertyItem>(section, section.Labels.First().ToString());
            item.Tooltip = TooltipMapper.Map(item.Access, item.Type, item.Name, string.Empty);
            item.Id = SyntaxMapper.MapId(item.FullName, null);
            item.Kind = CodeItemKindEnum.SwitchSection;
            item.Moniker = SyntaxMapper.MapMoniker(item.Kind, item.Access);

            return item;
        }
    }
}
