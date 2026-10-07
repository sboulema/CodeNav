using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CodeNav.OutOfProc.Helpers;

public static class SpanHelper
{
    /// <summary>
    /// Gets the zero-based line and column position for the start of the specified span.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree the span belongs to.</param>
    /// <param name="span">The span to resolve a line position for, or <see langword="null"/>.</param>
    /// <returns>
    /// The zero-based <see cref="LinePosition"/> for the start of <paramref name="span"/>,
    /// or <see langword="null"/> if <paramref name="span"/> is <see langword="null"/>.
    /// </returns>
    public static LinePosition? MapStartLinePosition(SyntaxTree? syntaxTree, TextSpan? span)
        => span == null || syntaxTree == null
            ? null
            : syntaxTree.GetLineSpan(span.Value).StartLinePosition;

    /// <summary>
    /// Gets the zero-based line and column position for the end of the specified span.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree the span belongs to.</param>
    /// <param name="span">The span to resolve a line position for, or <see langword="null"/>.</param>
    /// <returns>
    /// The zero-based <see cref="LinePosition"/> for the end of <paramref name="span"/>,
    /// or <see langword="null"/> if <paramref name="span"/> is <see langword="null"/>.
    /// </returns>
    public static LinePosition? MapEndLinePosition(SyntaxTree? syntaxTree, TextSpan? span)
        => span == null || syntaxTree == null
            ? null
            : syntaxTree.GetLineSpan(span.Value).EndLinePosition;

    /// <summary>
    /// Gets the zero-based line and column position for the start of the specified span.
    /// </summary>
    /// <param name="sourceText">The source text the span belongs to.</param>
    /// <param name="span">The span to resolve a line position for, or <see langword="null"/>.</param>
    /// <returns>
    /// The zero-based <see cref="LinePosition"/> for the start of <paramref name="span"/>,
    /// or <see langword="null"/> if <paramref name="span"/> is <see langword="null"/>.
    /// </returns>
    public static LinePosition? MapStartLinePosition(SourceText sourceText, TextSpan? span)
        => span == null
            ? null
            : sourceText.Lines.GetLinePositionSpan(span.Value).Start;

    /// <summary>
    /// Gets the zero-based line and column position for the end of the specified span.
    /// </summary>
    /// <param name="sourceText">The source text the span belongs to.</param>
    /// <param name="span">The span to resolve a line position for, or <see langword="null"/>.</param>
    /// <returns>
    /// The zero-based <see cref="LinePosition"/> for the end of <paramref name="span"/>,
    /// or <see langword="null"/> if <paramref name="span"/> is <see langword="null"/>.
    /// </returns>
    public static LinePosition? MapEndLinePosition(SourceText sourceText, TextSpan? span)
        => span == null
            ? null
            : sourceText.Lines.GetLinePositionSpan(span.Value).End;
}
