using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.OutOfProc.Languages.CSharp.Mappers;

public static class StructMapper
{
    public static CodeClassItem MapStruct(StructDeclarationSyntax member,
        SemanticModel semanticModel, SyntaxTree tree, CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeClassItem>(member, semanticModel, codeDocumentViewModel, member.Identifier, modifiers: member.Modifiers);
        codeItem.Kind = CodeItemKindEnum.Struct;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);
        codeItem.Tooltip = TooltipMapper.Map(member, codeItem.Access, string.Empty, codeItem.Name, codeItem.Parameters);

        foreach (var structMember in member.Members)
        {
            codeItem.Members.AddIfNotNull(DocumentMapper.MapMember(structMember, tree, semanticModel, codeDocumentViewModel));
        }

        return codeItem;
    }
}
