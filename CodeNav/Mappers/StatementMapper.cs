using System.Windows.Media;
using CodeNav.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeNav.Helpers;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic;

namespace CodeNav.Mappers
{
    /// <summary>
    /// Used to map the body of a method
    /// </summary>
    public static class StatementMapper
    {
        public static List<CodeItem> MapStatement(StatementSyntax statement, CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (statement == null) return new List<CodeItem>();

            switch (statement.Kind())
            {
                case SyntaxKind.SwitchStatement:
                    return new List<CodeItem> { MapSwitch(statement as SwitchStatementSyntax, control, semanticModel) };
                case SyntaxKind.Block:
                    return MapStatements((statement as BlockSyntax).Statements, control, semanticModel);
                case SyntaxKind.TryStatement:
                    return MapStatement((statement as TryStatementSyntax).Block, control, semanticModel);
                default:
                    return new List<CodeItem>();
            }
        }

        public static List<CodeItem> MapStatement(VisualBasicSyntax.StatementSyntax statement, CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (statement == null) return new List<CodeItem>();

            switch (statement.Kind())
            {
                case VisualBasic.SyntaxKind.SelectBlock:
                    return new List<CodeItem> { MapSwitch(statement as VisualBasicSyntax.SelectBlockSyntax, control, semanticModel) };
                case VisualBasic.SyntaxKind.TryStatement:
                    return MapStatement((statement as VisualBasicSyntax.TryBlockSyntax), control, semanticModel);
                default:
                    return new List<CodeItem>();
            }
        }

        public static List<CodeItem> MapStatement(BlockSyntax statement, CodeViewUserControl control, SemanticModel semanticModel) 
            => MapStatement(statement as StatementSyntax, control, semanticModel);

        public static List<CodeItem> MapStatements(SyntaxList<StatementSyntax> statements, CodeViewUserControl control, SemanticModel semanticModel)
        {
            var list = new List<CodeItem>();

            if (!statements.Any()) return list;

            foreach (var statement in statements)
            {
                list.AddRange(MapStatement(statement, control, semanticModel));
            }

            return list;
        }

        public static List<CodeItem> MapStatement(SyntaxList<VisualBasicSyntax.StatementSyntax> statements, CodeViewUserControl control, SemanticModel semanticModel)
        {
            var list = new List<CodeItem>();

            if (!statements.Any()) return list;

            foreach (var statement in statements)
            {
                list.AddRange(MapStatement(statement, control, semanticModel));
            }

            return list;
        }

        /// <summary>
        /// Map a switch statement
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="control"></param>
        /// <param name="semanticModel"></param>
        /// <returns></returns>
        private static CodeItem MapSwitch(SwitchStatementSyntax statement, CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (statement == null) return null;

            var item = BaseMapper.MapBase<CodeClassItem>(statement, statement.Expression.ToString(), control, semanticModel);
            item.Name = $"Switch {item.Name}";
            item.Kind = CodeItemKindEnum.Switch;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.BorderBrush = ColorHelper.CreateSolidColorBrush(Colors.DarkGray);
            item.Tooltip = TooltipMapper.Map(item.Access, string.Empty, item.Name, item.Parameters);

            // Map switch cases
            foreach (var section in statement.Sections)
            {
                item.Members.Add(MapSwitchSection(section, control, semanticModel));
            }

            return item;
        }

        private static CodeItem MapSwitch(VisualBasicSyntax.SelectBlockSyntax statement, CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (statement == null) return null;

            var item = BaseMapper.MapBase<CodeClassItem>(statement, statement.SelectStatement.Expression.ToString(), control, semanticModel);
            item.Name = $"Switch {item.Name}";
            item.Kind = CodeItemKindEnum.Switch;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.BorderBrush = ColorHelper.CreateSolidColorBrush(Colors.DarkGray);
            item.Tooltip = TooltipMapper.Map(item.Access, string.Empty, item.Name, item.Parameters);

            // Map switch cases
            foreach (var section in statement.CaseBlocks)
            {
                item.Members.Add(MapSwitchSection(section, control, semanticModel));
            }

            return item;
        }

        /// <summary>
        /// Map the individual cases within a switch statement
        /// </summary>
        /// <param name="section"></param>
        /// <param name="control"></param>
        /// <param name="semanticModel"></param>
        /// <returns></returns>
        private static CodeItem MapSwitchSection(SwitchSectionSyntax section, CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (section == null) return null;

            var item = BaseMapper.MapBase<CodePropertyItem>(section, section.Labels.First().ToString(), control, semanticModel);
            item.Tooltip = TooltipMapper.Map(item.Access, item.Type, item.Name, string.Empty);
            item.Id = item.FullName;
            item.Kind = CodeItemKindEnum.SwitchSection;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            return item;
        }

        private static CodeItem MapSwitchSection(VisualBasicSyntax.CaseBlockSyntax section, CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (section == null) return null;

            var item = BaseMapper.MapBase<CodePropertyItem>(section, section.CaseStatement.Cases.First().ToString(), control, semanticModel);
            item.Tooltip = TooltipMapper.Map(item.Access, item.Type, item.Name, string.Empty);
            item.Id = item.FullName;
            item.Kind = CodeItemKindEnum.SwitchSection;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            return item;
        }
    }
}
