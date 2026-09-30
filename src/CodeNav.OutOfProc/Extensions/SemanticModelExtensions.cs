using Microsoft.CodeAnalysis;

namespace CodeNav.OutOfProc.Extensions;

public static class SemanticModelExtensions
{
    public static SemanticModel? ForNode(this SemanticModel semanticModel, SyntaxNode node)
    {
        if (semanticModel.SyntaxTree == node.SyntaxTree)
        {
            return semanticModel;
        }

        return semanticModel.Compilation.ContainsSyntaxTree(node.SyntaxTree)
            ? semanticModel.Compilation.GetSemanticModel(node.SyntaxTree)
            : null;
    }
}
