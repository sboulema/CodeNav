using CodeNav.Models;
using Community.VisualStudio.Toolkit;
using System.Threading.Tasks;

namespace CodeNav.Helpers
{
    public static class LanguageHelper
    {
        /// <summary>
        /// Convert a language string to a strong typed language enum
        /// </summary>
        /// <param name="language">string representing the syntaxnode language</param>
        /// <returns></returns>
        public static LanguageEnum GetLanguage(string language)
        {
            switch (language)
            {
                case "Basic":
                case "Visual Basic":
                    return LanguageEnum.VisualBasic;
                case "C#":
                case "CSharp":
                    return LanguageEnum.CSharp;
                default:
                    return LanguageEnum.Unknown;
            }
        }

        public static async Task<LanguageEnum> GetActiveDocumentLanguage()
        {
            var textDocument = await VS.Editor.GetActiveTextDocumentAsync();
            return GetLanguage(textDocument?.Language);
        }
    }
}
