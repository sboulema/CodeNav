using CodeNav.Models;

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
                    return LanguageEnum.CSharp;
                default:
                    return LanguageEnum.Unknown;
            }
        }
    }
}
