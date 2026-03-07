namespace CodeNav.Test.Files;

class TestMethodsLocal
{
    public void Method()
    {

#pragma warning disable CS8321 // Local function is declared but never used
        void LocalMethod()
        {

        }
#pragma warning restore CS8321 // Local function is declared but never used
    }
}
