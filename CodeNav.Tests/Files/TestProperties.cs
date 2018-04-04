using System;

namespace CodeNav.Tests.Files
{
    public class TestProperties
    {
        public int PropertyGetSet { get; set; }
        public int PropertyGet { get; }
        public int PropertySet
        {
            set
            {
                
            }
        }

        private int _property;
        public int Property => _property; 
    }
}
