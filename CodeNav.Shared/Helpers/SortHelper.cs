using System.Collections.Generic;
using System.Linq;
using CodeNav.Models;
using CodeNav.Models.ViewModels;

namespace CodeNav.Helpers
{
    public static class SortHelper
    {
        public static List<CodeItem> Sort(CodeDocumentViewModel viewModel)
            => Sort(viewModel.CodeDocument, viewModel.SortOrder);

        public static List<CodeItem> Sort(List<CodeItem> document, SortOrderEnum sortOrder)
            => sortOrder switch
            {
                SortOrderEnum.SortByFile => SortByFile(document),
                SortOrderEnum.SortByName => SortByName(document),
                _ => document,
            };

        private static List<CodeItem> SortByName(List<CodeItem> document)
        {
            document = document.OrderBy(c => c.Name).ToList();

            foreach (var item in document)
            {
                if (item is CodeClassItem codeClassItem)
                {
                    codeClassItem.Members = SortByName(codeClassItem.Members);
                }
                if (item is CodeNamespaceItem codeNamespaceItem)
                {
                    codeNamespaceItem.Members = SortByName(codeNamespaceItem.Members);
                }
            }

            return document;
        }

        private static List<CodeItem> SortByFile(List<CodeItem> document)
        {
            document = document.OrderBy(c => c.StartLine).ToList();

            foreach (var item in document)
            {
                if (item is CodeClassItem codeClassItem)
                {
                    codeClassItem.Members = SortByFile(codeClassItem.Members);
                }
                if (item is CodeNamespaceItem codeNamespaceItem)
                {
                    codeNamespaceItem.Members = SortByFile(codeNamespaceItem.Members);
                }
            }

            return document;
        }
    }
}
