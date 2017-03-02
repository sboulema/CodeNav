using CodeNav.Mappers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests.MapperTests
{
    [TestFixture]
    public class TooltipMapperTests
    {
        [TestCase(CodeItemAccessEnum.Public, "int", "Property", "{get, set}", "Public int Property {get, set}")]
        [TestCase(CodeItemAccessEnum.Private, "string", "Property", "{get}", "Private string Property {get}")]
        [TestCase(CodeItemAccessEnum.Unknown, "int", "Property", "{set}", "int Property {set}")]
        [TestCase(CodeItemAccessEnum.Public, "", "Constructor", "", "Public Constructor")]
        [TestCase(CodeItemAccessEnum.Private, "List<Item>", "Property", "", "Private List<Item> Property")]
        [TestCase(CodeItemAccessEnum.Private, "System.Generic.Collection.List<Models.item>", "", "Helpers.Property", 
            "Private System.Generic.Collection.List<Models.item> Helpers.Property")]
        public void ShouldMapMethodOk(CodeItemAccessEnum access, string type, string name, string extra, string expected)
        {
            var tooltip = TooltipMapper.Map(access, type, name, extra);
            Assert.AreEqual(expected, tooltip);
        }
    }
}
