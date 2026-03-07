using Microsoft.VisualStudio.Extensibility;

namespace CodeNav.Test;

[VisualStudioContribution]
internal class MockEntrypoint : Extension
{
    public override ExtensionConfiguration ExtensionConfiguration => null!;
}
