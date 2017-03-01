using System;
using System.Collections.Generic;
using CodeNav.Models;

namespace CodeNav.Tests
{
    public class CodeNavTest
    {
        public CodeNavTest()
        {

        }

        public const string Constant = "CodeNav";

        protected int Version = 1;

        public bool Field = true;
        
        private int _secret = 2;

        public int PublicMethod(int a, int b)
        {
            return a + b;
        }

        public int PublicMethod(int a, int b, int c)
        {
            // Overloading method
            return a + b + c;
        }

        private int PrivateMethod(int a, int b)
        {
            return a - b;
        }
        
        protected void ProtectedMethod(int a, int b)
        {
            
        }

        public float Property { get; set; }

        private void GetListOfStrings(List<string> lines) { }

        public List<CodeItem> ReturnListOfCodeItems()
        {
            return new List<CodeItem>();
        }

        public delegate void ChangedEventHandler(object sender, EventArgs e);
        protected event ChangedEventHandler Changed;

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

        public bool RegionMethod(bool overload)
        {
            return true;
        }

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
        int InterfaceMethod(int input);
        int InterfaceProperty { get; }
    }

    public class CodeNavTest2 : ICodeNavTest2
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
