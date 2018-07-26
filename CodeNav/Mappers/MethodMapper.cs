using CodeNav.Helpers;
using CodeNav.Models;
using CodeNav.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Imaging;
using System.Linq;
using System.Windows.Media;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.Mappers
{
    public static class MethodMapper
    {
        public static CodeItem MapMethod(MethodDeclarationSyntax member, CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null) return null;

            CodeItem item;

            var statementsCodeItems = StatementMapper.MapStatement(member.Body, control, semanticModel);

            if (VisibilityHelper.ShouldBeVisible(CodeItemKindEnum.Switch) && statementsCodeItems.Any())
            {
                // Map method as item containing statements
                item = BaseMapper.MapBase<CodeClassItem>(member, member.Identifier, member.Modifiers, control, semanticModel);
                ((CodeClassItem)item).Members.AddRange(statementsCodeItems);
                ((CodeClassItem)item).BorderBrush = ColorHelper.ToBrush(Colors.DarkGray);
            }
            else
            {
                // Map method as single item
                item = BaseMapper.MapBase<CodeFunctionItem>(member, member.Identifier, member.Modifiers, control, semanticModel);
                ((CodeFunctionItem)item).Type = TypeMapper.Map(member.ReturnType);
                ((CodeFunctionItem)item).Parameters = ParameterMapper.MapParameters(member.ParameterList);
                item.Tooltip = TooltipMapper.Map(item.Access, ((CodeFunctionItem)item).Type, item.Name, member.ParameterList);
            }

            item.Id = IdMapper.MapId(item.FullName, member.ParameterList);
            item.Kind = CodeItemKindEnum.Method;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            if (TriviaSummaryMapper.HasSummary(member) && Settings.Default.UseXMLComments)
            {
                item.Tooltip = TriviaSummaryMapper.Map(member);
            }
            
            return item;
        }

        public static CodeItem MapMethod(VisualBasicSyntax.MethodBlockSyntax member, CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null) return null;

            CodeItem item;

            var statementsCodeItems = StatementMapper.MapStatement(member.Statements, control, semanticModel);

            if (VisibilityHelper.ShouldBeVisible(CodeItemKindEnum.Switch) && statementsCodeItems.Any())
            {
                // Map method as item containing statements
                item = BaseMapper.MapBase<CodeClassItem>(member, member.SubOrFunctionStatement.Identifier,
                    member.SubOrFunctionStatement.Modifiers, control, semanticModel);
                ((CodeClassItem)item).Members.AddRange(statementsCodeItems);
                ((CodeClassItem)item).BorderBrush = ColorHelper.ToBrush(Colors.DarkGray);
            }
            else
            {
                // Map method as single item
                item = BaseMapper.MapBase<CodeFunctionItem>(member, member.SubOrFunctionStatement.Identifier,
                    member.SubOrFunctionStatement.Modifiers, control, semanticModel);

                var symbol = semanticModel.GetDeclaredSymbol(member) as IMethodSymbol;
                ((CodeFunctionItem)item).Type = TypeMapper.Map(symbol?.ReturnType);
                ((CodeFunctionItem)item).Parameters = ParameterMapper.MapParameters(member.SubOrFunctionStatement.ParameterList, semanticModel);
                item.Tooltip = TooltipMapper.Map(item.Access, ((CodeFunctionItem)item).Type, item.Name, 
                    member.SubOrFunctionStatement.ParameterList, semanticModel);
            }

            item.Id = IdMapper.MapId(item.FullName, member.SubOrFunctionStatement.ParameterList, semanticModel);
            item.Kind = CodeItemKindEnum.Method;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            if (TriviaSummaryMapper.HasSummary(member) && Settings.Default.UseXMLComments)
            {
                item.Tooltip = TriviaSummaryMapper.Map(member);
            }

            return item;
        }

        public static CodeItem MapConstructor(VisualBasicSyntax.ConstructorBlockSyntax member, CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeFunctionItem>(member, member.SubNewStatement.NewKeyword, member.SubNewStatement.Modifiers, control, semanticModel);
            item.Parameters = ParameterMapper.MapParameters(member.SubNewStatement.ParameterList, semanticModel);
            item.Tooltip = TooltipMapper.Map(item.Access, item.Type, item.Name, member.SubNewStatement.ParameterList, semanticModel);
            item.Id = IdMapper.MapId(member.SubNewStatement.NewKeyword, member.SubNewStatement.ParameterList, semanticModel);
            item.Kind = CodeItemKindEnum.Constructor;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.OverlayMoniker = KnownMonikers.Add;

            return item;
        }

        public static CodeItem MapConstructor(ConstructorDeclarationSyntax member, CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null) return null;

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
