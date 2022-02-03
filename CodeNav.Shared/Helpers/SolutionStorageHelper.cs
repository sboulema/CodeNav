using CodeNav.Models;
using CodeNav.Models.ViewModels;
using Community.VisualStudio.Toolkit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodeNav.Helpers
{
    public static class SolutionStorageHelper
    {
        private const string ApplicationName = "CodeNav";

        public static async Task SaveToSolutionStorage(CodeDocumentViewModel codeDocumentViewModel)
        {
            var solutionStorageModel = await Load<SolutionStorageModel>();

            if (solutionStorageModel.Documents == null)
            {
                solutionStorageModel.Documents = new List<CodeDocumentViewModel>();
            }

            var storageItem = await GetStorageItem(codeDocumentViewModel.FilePath);
            solutionStorageModel.Documents.Remove(storageItem);

            solutionStorageModel.Documents.Add(codeDocumentViewModel);

            await Save(solutionStorageModel);
        }

        public static async Task<CodeDocumentViewModel> GetStorageItem(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            var solutionStorage = await Load<SolutionStorageModel>();

            if (solutionStorage?.Documents?.Any() != true)
            {
                return null;
            }

            var storageItem = solutionStorage.Documents
                .Where(item => !string.IsNullOrEmpty(item.FilePath))
                .FirstOrDefault(item => item.FilePath.Equals(filePath));

            return storageItem;
        }

        private static async Task<T> Load<T>()
        {
            try
            {
                var settingFilePath = await GetSettingsFilePath();

                if (!File.Exists(settingFilePath))
                {
                    return (T)Activator.CreateInstance(typeof(T));
                }

                using (var stream = new FileStream(settingFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream))
                {
                    return (T)new JsonSerializer().Deserialize(reader, typeof(T));
                }
            }
            catch (Exception e)
            {
                LogHelper.Log("Error deserializing Solution Storage", e);
            }

            return (T)Activator.CreateInstance(typeof(T));
        }

        private static async Task Save<T>(T storage)
        {
            try
            {
                var settingFilePath = await GetSettingsFilePath();

                if (!File.Exists(settingFilePath))
                {
                    return;
                }

                using (var stream = new FileStream(settingFilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                using (var writer = new StreamWriter(stream))
                {
                    new JsonSerializer().Serialize(writer, storage);
                }
            }
            catch (Exception e)
            {
                LogHelper.Log("Error serializing Solution Storage", e);
            }
        }

        private static async Task<string> GetSettingsFilePath()
        {
            var solution = await VS.Solutions.GetCurrentSolutionAsync();
            var solutionFilePath = solution.FullPath;

            if (!File.Exists(solutionFilePath))
            {
                return string.Empty;
            }

            var solutionFolder = Path.GetDirectoryName(solutionFilePath);
            return Path.Combine(solutionFolder, ".vs", $"{ApplicationName}.json");
        }
    }
}
