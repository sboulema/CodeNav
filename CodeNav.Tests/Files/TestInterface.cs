namespace CodeNav.Tests.Files
{
    interface ITestInterface
    {
        int InterfaceMethod();
        int InterfaceMethod(int input);
        int InterfaceProperty { get; }
    }

    public class ImplementingClass : ITestInterface
    {
        public int InterfaceMethod()
        {
            return 0;
        }

        public int InterfaceMethod(int input)
        {
            // Overloading within the same interface
            return input;
        }

        public int InterfaceMethod(int a, int b)
        {
            // Overloading outside the interface
            return a + b;
        }

        public void NonInterfaceMethod()
        {

        }

        public int InterfaceProperty { get; }
    }

    public class ImplementingClass2 : ITestInterface
    {
        #region ITestInterface implementation

        public int InterfaceMethod()
        {
            return 0;
        }

        public int InterfaceMethod(int input)
        {
            // Overloading within the same interface
            return input;
        }

        public int InterfaceProperty { get; }

        #endregion

        public int InterfaceMethod(int a, int b)
        {
            // Overloading outside the interface
            return a + b;
        }

        public void NonInterfaceMethod()
        {

        }
    }
}
