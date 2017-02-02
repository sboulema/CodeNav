using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNav.Tests
{
    interface ICodeNavTestInterface
    {
        string Area { get; }
        string Name { get; }
        string Value { get; }
        DateTime LastChanged { get; }
    }
}
