# Spans

A code item has a set of different span, each with their own start, end and length.
Each of these spans have their own specific purpose in CodeNav.
This document tries to clarify the differences.

## Span

The main span encapsulates the entire code member.

The main span is used when determining if a code item is part of a region or implemented interface.
And when selecting the code from the code item context menu.

<pre>
<font color="green">namespace CodeNav.Test.Files;

internal class TestSpans
{
    private int Counter = 0;
}</font>
</pre>

## Identifier span

The idemtifier span starts over the access and modifiers and runs to the end of the line,
including any parameters.

The identifier span is used when clicking on a code item and scrolling to and selecting a
member in the text view.

<pre>
namespace CodeNav.Test.Files;

internal class <font color="green">TestSpans</font>
{
    private int Counter = 0;
}
</pre>

## Outline span

The outline span starts after the identifier has ended and runs to the end of the span.

The outline span is used when expanding and collapsing a code item,
to find the matching outline region in the text view.

<pre>
namespace CodeNav.Test.Files;

internal class TestSpans<font color="green">
{
    private int Counter = 0;
}</font>
</pre>