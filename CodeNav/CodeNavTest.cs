using System.Collections.Generic;

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

        private void GetListOfStrings(List<string> lines) { }

        #region Region

        public bool RegionMethod()
        {
            return false;
        }

        public int RegionProperty
        {
            get; set;
        }

        public const string RegionString = "CodeNav";

        #endregion

        public struct Structure
        {
            public int StructureProperty { get; }
            public const int StructureConstant = 42;

            private void StructureMethod()
            {              
            }
        }

        internal class InternalClass
        {
            internal const int InternalConstant = 42;
            internal int InternalMethod()
            {
                return 42;
            }

            internal string InternalProperty { get; set; }
        }
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
