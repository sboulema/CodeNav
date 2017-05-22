namespace CodeNav.Tests.Files
{
    public class Class2
    {
        #region R1

        #region R15
        public const int nestedRegionConstant = 1;

        #region R16
        int i1 = 0;
        #endregion

        #endregion

        public void Test1()
        {
            #region R11
            int i1 = 0;
            #endregion

            #region R12
            int i2 = 0;
            #endregion

            #region R13
            int i3 = 0;
            #endregion

            #region R14
            int i4 = 0;
            #endregion
        }

        public void Test2()
        {

        }

        #endregion

        public void OutsideRegionFunction()
        {

        }

        #region R2
        public void SiblingRegionFunction()
        {

        }
        #endregion
    }
}
