namespace CodeNav.Tests.Files
{
    interface Interface2
    {
        int Interface2Method();
    }

    interface Interface1 : Interface2
    {
        int Interface1Method();
    }

    class ClassB : Interface1
    {
        public int Interface1Method()
        {
            throw new System.NotImplementedException();
        }

        public int Interface2Method()
        {
            throw new System.NotImplementedException();
        }
    }

    class ClassA : ClassB, Interface1
    {
        public void ClassAMethod() { }
    }
}
