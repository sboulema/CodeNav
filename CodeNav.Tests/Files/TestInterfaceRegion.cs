namespace CodeNav.Tests.Files
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
