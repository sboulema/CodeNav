using System;
using Microsoft.Build.Framework.XamlTypes;

namespace CodeNav.Tests
{
    public sealed class SealedBaseClass
    {
        public void Display()
        {
            Console.WriteLine("This is a sealed class which can;t be further inherited");
        }
    }

    public class BaseClass
    {
        public virtual void BaseMethod() { }
        public virtual int BaseProperty { get; set; }
    }

    public class InheritingClass : BaseClass
    {
        public sealed override void BaseMethod()
        {
        }

        public sealed override int BaseProperty { get; set; }
    }
}
