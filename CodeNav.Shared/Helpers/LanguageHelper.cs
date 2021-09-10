using CodeNav.Models;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.LanguageServices;
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
            var document = await DocumentHelper.GetCodeAnalysisDocument();

            if (document == null)
            {
                return LanguageEnum.Unknown;
            }

            var tree = await document.GetSyntaxTreeAsync();

            if (tree == null)
            {
                return LanguageEnum.Unknown;
            }

            var root = await tree.GetRootAsync();

            if (root == null)
            {
                return LanguageEnum.Unknown;
            }

            return GetLanguage(root.Language);
        }
    }
}
