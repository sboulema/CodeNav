using System;

namespace CodeNav.Tests.Files
{
    public interface IPartial1
    {
        void Partial1Method();
    }

    public partial class TestPartial : IPartial1
    {
        public void Partial1Method()
        {
            throw new NotImplementedException();
        }
    }
}
