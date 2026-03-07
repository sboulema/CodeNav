namespace CodeNav.Test.Files;

public sealed class SealedBaseClass
{
    public static void Display()
    {
        Console.WriteLine("This is a sealed class which can't be further inherited");
    }
}

public class NonSealedBaseClass
{
    public virtual void BaseMethod() { }

    public virtual int BaseProperty { get; set; }
}

public class InheritingClass : NonSealedBaseClass
{
    public sealed override void BaseMethod()
    {
    }

    public sealed override int BaseProperty { get; set; }
}
