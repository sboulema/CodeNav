namespace CodeNav.Test.Files.Interface
{
    interface IRegionTestInterface
    {
        int InterfaceMethod();
        int InterfaceMethod(int input);
        int InterfaceProperty { get; }

        #region
        int RegionMethod();
        #endregion
    }
}
