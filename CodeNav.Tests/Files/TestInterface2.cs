namespace CodeNav.Tests.Files
{
    interface ITestInterface1
    {
        int InterfaceMethod1();
    }

    interface ITestInterface2 : ITestInterface1
    {
        int InterfaceMethod2();
    }

    class ImplementingClass1 : ITestInterface2
    {
        public int InterfaceMethod1()
        {
            return 0;
        }

        public int InterfaceMethod2()
        {
            return 0;
        }
    }
}
