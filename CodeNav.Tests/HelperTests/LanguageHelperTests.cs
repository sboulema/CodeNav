using CodeNav.Helpers;
using CodeNav.Models;
using NUnit.Framework;

namespace CodeNav.Tests.HelperTests
{
    [TestFixture]
    public class LanguageHelperTests
    {
        [TestCase(LanguageEnum.CSharp, "C#")]
        [TestCase(LanguageEnum.VisualBasic, "Basic")]
        [TestCase(LanguageEnum.VisualBasic, "Visual Basic")]
        [TestCase(LanguageEnum.Unknown, "F#")]
        public void ItemsSorting(LanguageEnum expectedLanguage, string languageName)
        {
            Assert.AreEqual(expectedLanguage, LanguageHelper.GetLanguage(languageName));
        }
    }
}
