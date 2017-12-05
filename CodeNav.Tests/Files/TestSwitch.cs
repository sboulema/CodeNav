using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNav.Tests.Files
{
    class TestSwitch
    {
        void TestSwitchFunction()
        {
            var i = 0;

            switch (i)
            {
                case 0:
                    return;
                case 1:
                    return;
                default:
                    return;
            }
        }

        void TestSwitchInBlockFunction()
        {
            var i = 0;

            {
                switch (i)
                {
                    case 0:
                        return;
                    case 1:
                        return;
                    default:
                        return;
                }
            }         
        }

        void TestSwitchInTryCatchFunction()
        {
            var i = 0;

            try {
                switch (i)
                {
                    case 0:
                        return;
                    case 1:
                        return;
                    default:
                        return;
                }
            }
            catch(Exception)
            {

            }
        }
    }
}
