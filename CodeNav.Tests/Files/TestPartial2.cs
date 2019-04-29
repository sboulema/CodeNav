using System;

namespace CodeNav.Tests.Files
{
    public interface IPartial2
    {
        void Partial2Method();
    }

    public partial class TestPartial : IPartial2
    {
        public void Partial2Method()
        {
            throw new NotImplementedException();
        }
    }
}
