using System;
using System.Collections.Generic;
using System.Windows.Media;
using CodeNav.Helpers;
using CodeNav.Models;
using CodeNav.Properties;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.Mappers
{
    public static class SyntaxMapper
    {
        private static CodeViewUserControl _control;
        private static SyntaxTree _tree;

        public static List<CodeItem> MapDocument(Document document, CodeViewUserControl control)
        {
            _control = control;

            var text = GetText(document);
            _tree = CSharpSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)_tree.GetRoot();
            var result = new List<CodeItem>();

            foreach (var member in root.Members)
            {
                result.Add(MapMember(member));
            }

            return result;
        }

        private static CodeItem MapMember(MemberDeclarationSyntax member)
        {
            switch (member.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return MapMethod(member);
                //case vsCMElement.vsCMElementEnum:
                //    return MapEnum(element);
                //case vsCMElement.vsCMElementInterface:
                //    return MapInterface(element);
                //case vsCMElement.vsCMElementVariable:
                //    return MapVariable(element);
                //case vsCMElement.vsCMElementProperty:
                //    return MapProperty(element);
                //case vsCMElement.vsCMElementStruct:
                //    return MapStruct(element);
                //case vsCMElement.vsCMElementClass:
                //    return MapClass(element);
                //case vsCMElement.vsCMElementEvent:
                //    return MapEvent(element);
                //case vsCMElement.vsCMElementDelegate:
                //    return MapDelegate(element);
                case SyntaxKind.NamespaceDeclaration:
                    return MapNamespace(member as NamespaceDeclarationSyntax);
                default:
                    return null;
            }
        }

        private static T MapBase<T>(MemberDeclarationSyntax source, NameSyntax name) where T : CodeItem
        {
            var element = Activator.CreateInstance<T>();

            element.Name = name.ToString();
            element.FullName = name.ToString();
            element.Id = name.ToString();
            element.Tooltip = name.ToString();
            element.StartLine = _tree.GetLineSpan(source.Span).StartLinePosition.Line;
            element.Foreground = CreateSolidColorBrush(Colors.Black);
            
            //element.Access = MapAccessToEnum(source);

            element.FontSize = Settings.Default.Font.SizeInPoints;
            element.ParameterFontSize = Settings.Default.Font.SizeInPoints - 1;
            element.FontFamily = new FontFamily(Settings.Default.Font.FontFamily.Name);
            element.FontStyle = FontStyleMapper.Map(Settings.Default.Font.Style);

            element.Control = _control;

            return element;
        }

        private static CodeNamespaceItem MapNamespace(NamespaceDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = MapBase<CodeNamespaceItem>(member, member.Name);
            foreach (var namespaceMember in member.Members)
            {
                item.Members.Add(MapMember(namespaceMember));
            }
            return item;
        }

        private static CodeItem MapMethod(MemberDeclarationSyntax member)
        {
            var method = member;
            if (method == null) return null;

            //var item = MapBase<CodeFunctionItem>(element);
            //item.Type = MapReturnType(method.Type);
            //item.Parameters = MapParameters(method);
            //item.IconPath = method.FunctionKind == vsCMFunction.vsCMFunctionConstructor
            //    ? "pack://application:,,,/CodeNav;component/Icons/Method/MethodAdded_16x.xaml"
            //    : MapIcon<CodeFunction>(element);
            //item.Tooltip = $"{item.Access} {method.Type.AsString} {item.Name}{MapParameters(method, true)}";
            //item.Id = MapId(element);
            //item.Kind = method.FunctionKind == vsCMFunction.vsCMFunctionConstructor
            //    ? CodeItemKindEnum.Constructor
            //    : CodeItemKindEnum.Method;

            //return item;

            return null;
        }

        private static string GetText(Document document)
        {
            var doc = (TextDocument)document.Object("TextDocument");
            var p = doc.StartPoint.CreateEditPoint();
            return p.GetText(doc.EndPoint);
        }

        private static SolidColorBrush CreateSolidColorBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
