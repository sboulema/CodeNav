namespace CodeNav
{
    public class CodeNavTest
    {
        public CodeNavTest()
        {
            
        }

        public const string Constant = "CodeNav";

        protected int Version = 1;

        public bool Boolean = true;

        private int _secret = 2;

        public int Add(int a, int b)
        {
            return a + b;
        }

        private int Subtract(int a, int b)
        {
            return a - b;
        }

        protected void Divide(int a, int b)
        {
            
        }

        public float Property { get; set; }

        #region Region

        public bool RegionMethod()
        {
            return false;
        }  

        #endregion
    }

    public interface ICodeNavTest2
    {
        int InterfaceMethod();
    }

    public class CodeNavTest2 : ICodeNavTest2
    {
        public int InterfaceMethod()
        {
            return 0;
        }

        public void NonInterfaceMethod()
        {
            
        }
    }

    public enum DayEnum
    {
        Monday,
        Tueday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }
}
