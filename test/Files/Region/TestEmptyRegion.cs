namespace CodeNav.Test.Files.Region;

internal class TestEmptyRegions
{
    #region Const and Static
    const int DEFAULT_ID = 42;
    #endregion

    #region Fields
    private readonly DateTime _someDate;
    #endregion

    #region Properties
    #endregion

    #region Constructors & Destructors
    public TestEmptyRegions()
    {
        _someDate = DateTime.Now;
    }
    #endregion
}