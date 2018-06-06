using CodeNav.Helpers;
using CodeNav.Models;
using CodeNav.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Media;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.Mappers
{
    public class InterfaceMapper
    {
        public static bool IsPartOfImplementedInterface(IEnumerable<CodeImplementedInterfaceItem> implementedInterfaces, CodeItem item)
        {
            return item != null && implementedInterfaces.SelectMany(i => i.Members.Select(m => m.Id)).Contains(item.Id);
        }

        public static List<CodeImplementedInterfaceItem> MapImplementedInterfaces(SyntaxNode member,
            SemanticModel semanticModel)
        {
            var implementedInterfaces = new List<CodeImplementedInterfaceItem>();

            INamedTypeSymbol classSymbol;
            try
            {
                classSymbol = semanticModel.GetDeclaredSymbol(member) as INamedTypeSymbol;
            }
            catch (Exception e)
            {
                LogHelper.Log($"Error during mapping: MapImplementedInterface: {e.Message}");
                return implementedInterfaces;
            }

            if (classSymbol == null) return implementedInterfaces;

            var interfacesList = new List<INamedTypeSymbol>();
            GetInterfaces(interfacesList, classSymbol.Interfaces);

            foreach (var implementedInterface in interfacesList.Distinct())
            {
                implementedInterfaces.Add(MapImplementedInterface(implementedInterface.Name, implementedInterface.GetMembers(), classSymbol));
            }

            return implementedInterfaces;
        }

        /// <summary>
        /// Recursively get the interfaces implemented by the class.
        /// This ignores interfaces implemented by any base class, contrary to the .Allinterfaces behaviour
        /// </summary>
        /// <param name="interfacesFound">List of all interfaces found</param>
        /// <param name="source">Implemented interfaces</param>
        private static void GetInterfaces(List<INamedTypeSymbol> interfacesFound, ImmutableArray<INamedTypeSymbol> source)
        {
            interfacesFound.AddRange(source);
            foreach (var interfaceItem in source)
            {
                GetInterfaces(interfacesFound, interfaceItem.Interfaces);
            }
        }

        public static CodeImplementedInterfaceItem MapImplementedInterface(string name,
            ImmutableArray<ISymbol> members, INamedTypeSymbol implementingClass)
        {
            var item = new CodeImplementedInterfaceItem
            {
                Name = name,
                FullName = name,
                Id = name,
                Foreground = ColorHelper.ToBrush(Colors.Black),
                BorderBrush = ColorHelper.ToBrush(Colors.DarkGray),
                FontSize = Settings.Default.Font.SizeInPoints - 2,
                Kind = CodeItemKindEnum.ImplementedInterface,
                IsExpanded = true
            };

            foreach (var member in members)
            {
                var implementation = implementingClass.FindImplementationForInterfaceMember(member);
                if (implementation == null || !implementation.DeclaringSyntaxReferences.Any()) continue;
                var reference = implementation.DeclaringSyntaxReferences.First();
                var declarationSyntax = reference.GetSyntax();

                var interfaceMember = SyntaxMapper.MapMember(declarationSyntax as MemberDeclarationSyntax);
                if (interfaceMember == null) continue;

                interfaceMember.OverlayMoniker = KnownMonikers.InterfacePublic;
                item.Members.Add(interfaceMember);
            }

            if (item.Members.Any())
            {
                item.StartLine = item.Members.Min(i => i.StartLine);
                item.EndLine = item.Members.Max(i => i.EndLine);
            }

            return item;
        }

        public static CodeInterfaceItem MapInterface(InterfaceDeclarationSyntax member, 
            CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeInterfaceItem>(member, member.Identifier, member.Modifiers, control, semanticModel);
            item.Kind = CodeItemKindEnum.Interface;
            item.BorderBrush = ColorHelper.ToBrush(Colors.DarkGray);
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            foreach (var interfaceMember in member.Members)
            {
                item.Members.Add(SyntaxMapper.MapMember(interfaceMember));
            }

            return item;
        }

        public static CodeInterfaceItem MapInterface(VisualBasicSyntax.InterfaceBlockSyntax member,
            CodeViewUserControl control, SemanticModel semanticModel)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeInterfaceItem>(member, member.InterfaceStatement.Identifier, 
                member.InterfaceStatement.Modifiers, control, semanticModel);
            item.Kind = CodeItemKindEnum.Interface;
            item.BorderBrush = ColorHelper.ToBrush(Colors.DarkGray);
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            foreach (var interfaceMember in member.Members)
            {
                item.Members.Add(SyntaxMapper.MapMember(interfaceMember));
            }

            return item;
        }
    }
}
