namespace CodeNav.Test.Files.Interface;

internal class TestInterfacesVsRegions : ITestInterfacesVsRegions
{
    #region PublicMethod1

    public void PublicMethod1()
    {
    }

    #endregion

    #region PublicMethod2

    public void PublicMethod2()
    {
    }

    #endregion
}

public interface ITestInterfacesVsRegions
{
    void PublicMethod1();

    void PublicMethod2();
}