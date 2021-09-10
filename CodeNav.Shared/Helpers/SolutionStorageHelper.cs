using CodeNav.Models;
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

        public static async Task<T> Load<T>()
        {
            try
            {
                var solution = await VS.Solutions.GetCurrentSolutionAsync();
                var solutionFilePath = solution.FullPath;

                if (!File.Exists(solutionFilePath))
                {
                    return (T)Activator.CreateInstance(typeof(T));
                }

                var solutionFolder = Path.GetDirectoryName(solutionFilePath);
                var settingFilePath = Path.Combine(solutionFolder, ".vs", $"{ApplicationName}.json");
                var oldSettingFilePath = Path.Combine(solutionFolder, $"{ApplicationName}.json");

                if (File.Exists(settingFilePath))
                {
                    var json = File.ReadAllText(settingFilePath);
                    return JsonConvert.DeserializeObject<T>(json);
                }

                if (File.Exists(oldSettingFilePath))
                {
                    var json = File.ReadAllText(oldSettingFilePath);
                    File.Delete(oldSettingFilePath);
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception e)
            {
                LogHelper.Log("Error deserializing Solution Storage", e);
            }

            return (T)Activator.CreateInstance(typeof(T));
        }

        public static async Task Save<T>(T storage)
        {
            var solution = await VS.Solutions.GetCurrentSolutionAsync();
            var solutionFilePath = solution.FullPath;

            if (!File.Exists(solutionFilePath))
            {
                return;
            }

            var json = JsonConvert.SerializeObject(storage);

            var solutionFolder = Path.GetDirectoryName(solutionFilePath);
            File.WriteAllText(Path.Combine(solutionFolder, ".vs", $"{ApplicationName}.json"), json);
        }

        public static async Task SaveToSolutionStorage(CodeDocumentViewModel codeDocumentViewModel)
        {
            var solutionStorageModel = await Load<SolutionStorageModel>();

            if (solutionStorageModel.Documents == null)
            {
                solutionStorageModel.Documents = new List<CodeDocumentViewModel>();
            }

            var storageItem = solutionStorageModel.Documents
                .FirstOrDefault(d => d.FilePath.Equals(codeDocumentViewModel.FilePath));
            solutionStorageModel.Documents.Remove(storageItem);

            solutionStorageModel.Documents.Add(codeDocumentViewModel);

            await Save(solutionStorageModel);
        }
    }
}
