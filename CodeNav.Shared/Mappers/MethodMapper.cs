#nullable enable

using CodeNav.Helpers;
using CodeNav.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Imaging;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.Mappers
{
    public static class MethodMapper
    {
        public static CodeItem? MapMethod(MethodDeclarationSyntax? member,
            ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null)
            {
                return null;
            }

            return MapMethod(member, member.Identifier, member.Modifiers,
                member.Body, member.ReturnType as ITypeSymbol, member.ParameterList,
               CodeItemKindEnum.Method, control, semanticModel);
        }

        public static CodeItem? MapMethod(LocalFunctionStatementSyntax member,
            ICodeViewUserControl control, SemanticModel semanticModel)
        {
            return MapMethod(member, member.Identifier, member.Modifiers,
                member.Body, member.ReturnType as ITypeSymbol, member.ParameterList,
               CodeItemKindEnum.LocalFunction, control, semanticModel);
        }

        private static CodeItem? MapMethod(SyntaxNode node, SyntaxToken identifier,
            SyntaxTokenList modifiers, BlockSyntax? body, ITypeSymbol? returnType,
            ParameterListSyntax parameterList, CodeItemKindEnum kind,
            ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (node == null)
            {
                return null;
            }

            CodeItem item;

            var statementsCodeItems = StatementMapper.MapStatement(body, control, semanticModel);

            VisibilityHelper.SetCodeItemVisibility(statementsCodeItems);

            if (statementsCodeItems.Any(statement => statement.IsVisible == Visibility.Visible))
            {
                // Map method as item containing statements
                item = BaseMapper.MapBase<CodeClassItem>(node, identifier,modifiers, control, semanticModel);
                ((CodeClassItem)item).Members.AddRange(statementsCodeItems);
                ((CodeClassItem)item).BorderColor = Colors.DarkGray;
            }
            else
            {
                // Map method as single item
                item = BaseMapper.MapBase<CodeFunctionItem>(node, identifier, modifiers, control, semanticModel);
                ((CodeFunctionItem)item).Type = TypeMapper.Map(returnType);
                ((CodeFunctionItem)item).Parameters = ParameterMapper.MapParameters(parameterList);
                item.Tooltip = TooltipMapper.Map(item.Access, ((CodeFunctionItem)item).Type, item.Name, parameterList);
            }

            item.Id = IdMapper.MapId(item.FullName, parameterList);
            item.Kind = kind;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            if (TriviaSummaryMapper.HasSummary(node) && SettingsHelper.UseXMLComments)
            {
                item.Tooltip = TriviaSummaryMapper.Map(node);
            }

            return item;
        }

        public static CodeItem? MapMethod(VisualBasicSyntax.MethodStatementSyntax? member,
            ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null)
            {
                return null;
            }

            var item = BaseMapper.MapBase<CodeFunctionItem>(member, member.Identifier, member.Modifiers, control, semanticModel);

            item.Id = IdMapper.MapId(item.FullName, member.ParameterList, semanticModel);
            item.Kind = CodeItemKindEnum.Method;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            if (TriviaSummaryMapper.HasSummary(member) && SettingsHelper.UseXMLComments)
            {
                item.Tooltip = TriviaSummaryMapper.Map(member);
            }

            return item;
        }

        public static CodeItem? MapMethod(VisualBasicSyntax.MethodBlockSyntax? member,
            ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null)
            {
                return null;
            }

            CodeItem item;

            var statementsCodeItems = StatementMapper.MapStatement(member.Statements, control, semanticModel);

            VisibilityHelper.SetCodeItemVisibility(statementsCodeItems);

            if (statementsCodeItems.Any(statement => statement.IsVisible == Visibility.Visible))
            {
                // Map method as item containing statements
                item = BaseMapper.MapBase<CodeClassItem>(member, member.SubOrFunctionStatement.Identifier,
                    member.SubOrFunctionStatement.Modifiers, control, semanticModel);
                ((CodeClassItem)item).Members.AddRange(statementsCodeItems);
                ((CodeClassItem)item).BorderColor = Colors.DarkGray;
            }
            else
            {
                // Map method as single item
                item = BaseMapper.MapBase<CodeFunctionItem>(member, member.SubOrFunctionStatement.Identifier,
                    member.SubOrFunctionStatement.Modifiers, control, semanticModel);

                var symbol = SymbolHelper.GetSymbol<IMethodSymbol>(semanticModel, member);
                ((CodeFunctionItem)item).Type = TypeMapper.Map(symbol?.ReturnType);
                ((CodeFunctionItem)item).Parameters = ParameterMapper.MapParameters(member.SubOrFunctionStatement.ParameterList, semanticModel);
                item.Tooltip = TooltipMapper.Map(item.Access, ((CodeFunctionItem)item).Type, item.Name,
                    member.SubOrFunctionStatement.ParameterList, semanticModel);
            }

            item.Id = IdMapper.MapId(item.FullName, member.SubOrFunctionStatement.ParameterList, semanticModel);
            item.Kind = CodeItemKindEnum.Method;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            if (TriviaSummaryMapper.HasSummary(member) && SettingsHelper.UseXMLComments)
            {
                item.Tooltip = TriviaSummaryMapper.Map(member);
            }

            return item;
        }

        public static CodeItem? MapConstructor(VisualBasicSyntax.ConstructorBlockSyntax? member,
            ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null)
            {
                return null;
            }

            var item = BaseMapper.MapBase<CodeFunctionItem>(member, member.SubNewStatement.NewKeyword, member.SubNewStatement.Modifiers, control, semanticModel);
            item.Parameters = ParameterMapper.MapParameters(member.SubNewStatement.ParameterList, semanticModel);
            item.Tooltip = TooltipMapper.Map(item.Access, item.Type, item.Name, member.SubNewStatement.ParameterList, semanticModel);
            item.Id = IdMapper.MapId(member.SubNewStatement.NewKeyword, member.SubNewStatement.ParameterList, semanticModel);
            item.Kind = CodeItemKindEnum.Constructor;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.OverlayMoniker = KnownMonikers.Add;

            return item;
        }

        public static CodeItem? MapConstructor(ConstructorDeclarationSyntax? member,
            ICodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null)
            {
                return null;
            }

            var item = BaseMapper.MapBase<CodeFunctionItem>(member, member.Identifier, member.Modifiers, control, semanticModel);
            item.Parameters = ParameterMapper.MapParameters(member.ParameterList);
            item.Tooltip = TooltipMapper.Map(item.Access, item.Type, item.Name, member.ParameterList);
            item.Id = IdMapper.MapId(member.Identifier, member.ParameterList);
            item.Kind = CodeItemKindEnum.Constructor;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.OverlayMoniker = KnownMonikers.Add;

            return item;
        }
    }
}
