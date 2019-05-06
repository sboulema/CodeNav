using CodeNav.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Zu.TypeScript.TsTypes;

namespace CodeNav.Mappers.JavaScript
{
    public static class FunctionMapperJS
    {
        public static List<CodeItem> MapFunction(FunctionDeclaration function, ICodeViewUserControl control)
        {
            return MapFunction(function, function.Parameters, function.IdentifierStr, control);
        }

        public static List<CodeItem> MapFunctionExpression(VariableDeclaration declarator, ICodeViewUserControl control)
        {
            var function = declarator.Initializer as FunctionExpression;

            return MapFunction(function, function.Parameters, declarator.IdentifierStr, control);
        }

        public static List<CodeItem> MapFunctionExpression(FunctionExpression function, ICodeViewUserControl control)
        {
            return MapFunction(function, function.Parameters, function.IdentifierStr, control);
        }

        public static List<CodeItem> MapArrowFunctionExpression(VariableDeclaration declarator, ICodeViewUserControl control)
        {
            var function = declarator.Initializer as ArrowFunction;

            return MapFunction(function, function.Parameters, declarator.IdentifierStr, control);
        }

        public static List<CodeItem> MapNewExpression(VariableDeclaration declarator, ICodeViewUserControl control)
        {
            var expression = declarator.Initializer as NewExpression;

            if (!expression.IdentifierStr?.Equals("Function") ?? false) return null;

            return MapFunction(expression, new NodeArray<ParameterDeclaration>(), declarator.IdentifierStr, control);
        }

        public static List<CodeItem> MapFunction(Node function, NodeArray<ParameterDeclaration> parameters, string id, ICodeViewUserControl control)
        {
            if (function == null) return null;

            List<CodeItem> children;

            try
            {
                children = function.Children
                .FirstOrDefault(c => c.Kind == SyntaxKind.Block)?.Children
                .SelectMany(SyntaxMapperJS.MapMember)
                .ToList();
            }
            catch (NullReferenceException)
            {
                return new List<CodeItem>();
            }

            if (children != null && children.Any())
            {
                SyntaxMapper.FilterNullItems(children);

                var item = BaseMapperJS.MapBase<CodeClassItem>(function, id, control);

                item.BorderColor = Colors.DarkGray;

                item.Kind = CodeItemKindEnum.Method;
                item.Parameters = $"({string.Join(", ", parameters.Select(p => p.IdentifierStr))})";
                item.Tooltip = TooltipMapper.Map(item.Access, null, item.Name, item.Parameters);
                item.Id = IdMapper.MapId(item.FullName, parameters);
                item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

                item.Members = children;

                return new List<CodeItem> { item };
            }

            CodeFunctionItem functionItem = BaseMapperJS.MapBase<CodeFunctionItem>(function, id, control);

            functionItem.Kind = CodeItemKindEnum.Method;
            functionItem.Parameters = $"({string.Join(", ", parameters.Select(p => p.IdentifierStr))})";
            functionItem.Tooltip = TooltipMapper.Map(functionItem.Access, null, functionItem.Name, functionItem.Parameters);
            functionItem.Id = IdMapper.MapId(functionItem.FullName, parameters);
            functionItem.Moniker = IconMapper.MapMoniker(functionItem.Kind, functionItem.Access);

            return new List<CodeItem> { functionItem };
        }
    }
}
