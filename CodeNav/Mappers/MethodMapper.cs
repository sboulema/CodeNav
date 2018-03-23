using CodeNav.Helpers;
using CodeNav.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
                ((CodeClassItem)item).BorderBrush = ColorHelper.CreateSolidColorBrush(Colors.DarkGray);
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
                ((CodeClassItem)item).BorderBrush = ColorHelper.CreateSolidColorBrush(Colors.DarkGray);
            }
            else
            {
                // Map method as single item
                item = BaseMapper.MapBase<CodeFunctionItem>(member, member.SubOrFunctionStatement.Identifier,
                    member.SubOrFunctionStatement.Modifiers, control, semanticModel);
                ((CodeFunctionItem)item).Type = TypeMapper.Map(member.SubOrFunctionStatement.AsClause.Type);
                ((CodeFunctionItem)item).Parameters = ParameterMapper.MapParameters(member.SubOrFunctionStatement.ParameterList);
                item.Tooltip = TooltipMapper.Map(item.Access, ((CodeFunctionItem)item).Type, item.Name, member.SubOrFunctionStatement.ParameterList);
            }

            item.Id = IdMapper.MapId(item.FullName, member.SubOrFunctionStatement.ParameterList);
            item.Kind = CodeItemKindEnum.Method;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            return item;
        }
    }
}
