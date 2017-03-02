using CodeNav.Mappers;
using NUnit.Framework;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TypeMapperTests
    {
        [TestCase("System.Generic.Collection.List<Models.item>", true, "System.Generic.Collection.List<Models.item>")]
        [TestCase("System.Generic.Collection.List<Models.item>", false, "List<item>")]
        [TestCase("CodeNav.Models.CodeItem", true, "CodeNav.Models.CodeItem")]
        [TestCase("CodeNav.Models.CodeItem", false, "CodeItem")]
        public void ShouldMapTypeOk(string type, bool useLongNames, string expected)
        {
            var actual = TypeMapper.Map(type, useLongNames);
            Assert.AreEqual(expected, actual);
        }
    }
}
