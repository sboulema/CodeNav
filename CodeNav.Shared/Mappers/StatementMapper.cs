#nullable enable

using System.Windows.Media;
using CodeNav.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic;
using CodeNav.Extensions;

namespace CodeNav.Mappers
{
    /// <summary>
    /// Used to map the body of a method
    /// </summary>
    public static class StatementMapper
    {
        public static List<CodeItem> MapStatement(StatementSyntax? statement, ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (statement == null)
            {
                return new List<CodeItem>();
            }

            CodeItem? item;

            switch (statement.Kind())
            {
                case SyntaxKind.SwitchStatement:
                    item = MapSwitch(statement as SwitchStatementSyntax, control, semanticModel);
                    return item != null ? new List<CodeItem> { item } : new List<CodeItem>();
                case SyntaxKind.Block:
                    if (!(statement is BlockSyntax blockSyntax))
                    {
                        return new List<CodeItem>();
                    }

                    return MapStatements(blockSyntax.Statements, control, semanticModel);
                case SyntaxKind.TryStatement:
                    if (!(statement is TryStatementSyntax trySyntax))
                    {
                        return new List<CodeItem>();
                    }

                    return MapStatement(trySyntax.Block, control, semanticModel);
                case SyntaxKind.LocalFunctionStatement:
                    if (!(statement is LocalFunctionStatementSyntax syntax))
                    {
                        return new List<CodeItem>();
                    }

                    item = MethodMapper.MapMethod(syntax, control, semanticModel);
                    return item != null ? new List<CodeItem> { item } : new List<CodeItem>();
                default:
                    return new List<CodeItem>();
            }
        }

        public static List<CodeItem> MapStatement(VisualBasicSyntax.StatementSyntax? statement, ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (statement == null)
            {
                return new List<CodeItem>();
            }

            switch (statement.Kind())
            {
                case VisualBasic.SyntaxKind.SelectBlock:
                    var item = MapSwitch(statement as VisualBasicSyntax.SelectBlockSyntax, control, semanticModel);
                    return item != null ? new List<CodeItem> { item } : new List<CodeItem>();
                case VisualBasic.SyntaxKind.TryStatement:
                    return MapStatement((statement as VisualBasicSyntax.TryBlockSyntax), control, semanticModel);
                default:
                    return new List<CodeItem>();
            }
        }

        public static List<CodeItem> MapStatement(BlockSyntax? statement, ICodeViewUserControl control, SemanticModel semanticModel) 
            => MapStatement(statement as StatementSyntax, control, semanticModel);

        public static List<CodeItem> MapStatements(SyntaxList<StatementSyntax> statements,
            ICodeViewUserControl control, SemanticModel semanticModel)
        {
            var list = new List<CodeItem>();

            if (statements.Any() != true)
            {
                return list;
            }

            foreach (var statement in statements)
            {
                list.AddRange(MapStatement(statement, control, semanticModel));
            }

            return list;
        }

        public static List<CodeItem> MapStatement(SyntaxList<VisualBasicSyntax.StatementSyntax> statements, ICodeViewUserControl control, SemanticModel semanticModel)
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
        private static CodeItem? MapSwitch(SwitchStatementSyntax? statement, ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (statement == null)
            {
                return null;
            }

            var item = BaseMapper.MapBase<CodeClassItem>(statement, statement.Expression.ToString(), control, semanticModel);
            item.Name = $"Switch {item.Name}";
            item.Kind = CodeItemKindEnum.Switch;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.BorderColor = Colors.DarkGray;
            item.Tooltip = TooltipMapper.Map(item.Access, string.Empty, item.Name, item.Parameters);

            // Map switch cases
            foreach (var section in statement.Sections)
            {
                item.Members.AddIfNotNull(MapSwitchSection(section, control, semanticModel));
            }

            return item;
        }

        private static CodeItem? MapSwitch(VisualBasicSyntax.SelectBlockSyntax? statement, ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (statement == null)
            {
                return null;
            }

            var item = BaseMapper.MapBase<CodeClassItem>(statement, statement.SelectStatement.Expression.ToString(), control, semanticModel);
            item.Name = $"Select {item.Name}";
            item.Kind = CodeItemKindEnum.Switch;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.BorderColor = Colors.DarkGray;
            item.Tooltip = TooltipMapper.Map(item.Access, string.Empty, item.Name, item.Parameters);

            // Map switch cases
            foreach (var section in statement.CaseBlocks)
            {
                item.Members.AddIfNotNull(MapSwitchSection(section, control, semanticModel));
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
        private static CodeItem? MapSwitchSection(SwitchSectionSyntax? section, ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (section == null)
            {
                return null;
            }

            var item = BaseMapper.MapBase<CodePropertyItem>(section, section.Labels.First().ToString(), control, semanticModel);
            item.Tooltip = TooltipMapper.Map(item.Access, item.Type, item.Name, string.Empty);
            item.Id = item.FullName;
            item.Kind = CodeItemKindEnum.SwitchSection;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            return item;
        }

        private static CodeItem? MapSwitchSection(VisualBasicSyntax.CaseBlockSyntax? section, ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (section == null)
            {
                return null;
            }

            var item = BaseMapper.MapBase<CodePropertyItem>(section, section.CaseStatement.Cases.First().ToString(), control, semanticModel);
            item.Tooltip = TooltipMapper.Map(item.Access, item.Type, item.Name, string.Empty);
            item.Id = item.FullName;
            item.Kind = CodeItemKindEnum.SwitchSection;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            return item;
        }
    }
}
