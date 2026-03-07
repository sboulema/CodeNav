namespace CodeNav.Test.Files.BaseClass;

// Base classs is declared in another file and NOT correctly mapped
internal class TestBaseClass : TestAbstractBaseClass
{
    public int Days { get; set; }
}

// Base class is declared in the same file and IS correctly mapped
internal class AnotherTestBaseClass : AnotherAbstractTestBaseClass
{
    public int Days { get; set; }
}

internal abstract class AnotherAbstractTestBaseClass
{
    public int Weeks { get; set; }
}
