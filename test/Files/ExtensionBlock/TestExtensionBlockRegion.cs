namespace CodeNav.Test.Files.ExtensionBlock;

internal static class TestExtensionBlockRegion
{
    extension(object obj)
    {
        #region Foo
        public bool IsNull()
        {
            return obj is null;
        }
        #endregion
    }
}
