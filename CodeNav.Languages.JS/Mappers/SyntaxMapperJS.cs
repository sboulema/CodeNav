#nullable enable

using CodeNav.Mappers;
using CodeNav.Models;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Zu.TypeScript;
using Zu.TypeScript.TsTypes;

namespace CodeNav.Languages.JS.Mappers
{
    public static class SyntaxMapperJS
    {
        private static ICodeViewUserControl? _control;

        public static List<CodeItem?> Map(Document document, ICodeViewUserControl control)
            => Map(document.FilePath, control);

        public static List<CodeItem?> Map(string? filePath, ICodeViewUserControl control)
        {
            _control = control;

            if (string.IsNullOrEmpty(filePath) ||
                !File.Exists(filePath))
            {
                return new List<CodeItem?>();
            }

            var jsString = File.ReadAllText(filePath);

            var ast = new TypeScriptAST(jsString, filePath);

            return new List<CodeItem?>
            {
                new CodeNamespaceItem
                {
                    Id = "Namespace" + filePath,
                    Kind = CodeItemKindEnum.Namespace,
                    Members = new List<CodeItem>
                    {
                        new CodeClassItem
                        {
                            Id = filePath!,
                            Kind = CodeItemKindEnum.Class,
                            Access = CodeItemAccessEnum.Public,
                            Moniker = IconMapper.MapMoniker(CodeItemKindEnum.Class, CodeItemAccessEnum.Public),
                            Name = Path.GetFileNameWithoutExtension(filePath),
                            BorderColor = Colors.DarkGray,
                            Members = MapMembers(ast)
                        }
                    }
                }
            };
        }

        public static List<CodeItem> MapMember(Node member)
        {
            switch (member.Kind)
            {
                case SyntaxKind.FunctionDeclaration:
                    return FunctionMapperJS.MapFunction(member as FunctionDeclaration, _control);
                case SyntaxKind.FunctionExpression:
                    return FunctionMapperJS.MapFunctionExpression(member as FunctionExpression, _control);
                case SyntaxKind.VariableStatement:
                    return MapVariable(member as VariableStatement);
                case SyntaxKind.ExpressionStatement:
                case SyntaxKind.PrefixUnaryExpression:
                case SyntaxKind.CallExpression:
                case SyntaxKind.PropertyAccessExpression:               
                    return MapChildren(member);
                case SyntaxKind.BinaryExpression:
                    return MapBinaryExpression(member as BinaryExpression);
                default:
                    break;
            }

            return new List<CodeItem>();
        }

        private static List<CodeItem> MapMembers(TypeScriptAST ast)
        {
            if (ast?.RootNode?.Children?.Any() != true)
            {
                return new List<CodeItem>();
            }

            var members = ast.RootNode.Children.SelectMany(MapMember);

            if (members == null)
            {
                return new List<CodeItem>();
            }

            return members.ToList();
        }

        private static List<CodeItem> MapBinaryExpression(BinaryExpression? expression)
        {
            if (expression == null)
            {
                return new List<CodeItem>();
            }

            if (expression.Right.Kind != SyntaxKind.FunctionExpression)
            {
                return new List<CodeItem>();
            }

            if (!(expression.Right is FunctionExpression function))
            {
                return new List<CodeItem>();
            }

            return FunctionMapperJS.MapFunction(function, function.Parameters, expression.First.IdentifierStr, _control);
        }

        private static List<CodeItem> MapVariable(VariableStatement? variable)
        {
            if (variable == null)
            {
                return new List<CodeItem>();
            }

            var declarator = variable.DeclarationList.Declarations.First();

            if (declarator.Initializer != null)
            {
                switch (declarator.Initializer.Kind)
                {
                    case SyntaxKind.FunctionExpression:
                        return FunctionMapperJS.MapFunctionExpression(declarator, _control);
                    case SyntaxKind.ArrowFunction:
                        return FunctionMapperJS.MapArrowFunctionExpression(declarator, _control);
                    case SyntaxKind.NewExpression:
                        return FunctionMapperJS.MapNewExpression(declarator, _control);
                    default:
                        break;
                }
            }

            if (variable.Parent.Kind != SyntaxKind.SourceFile)
            {
                return new List<CodeItem>();
            }

            var item = BaseMapperJS.MapBase<CodeItem>(variable, declarator.IdentifierStr, _control);
            item.Kind = CodeItemKindEnum.Variable;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            return new List<CodeItem> { item };
        }

        private static List<CodeItem> MapChildren(Node member)
        {
            return member.Children.SelectMany(MapMember).ToList();
        }
    }
}
