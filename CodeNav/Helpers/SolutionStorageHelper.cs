using CodeNav.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeNav.Helpers
{
    public static class SolutionStorageHelper
    {
        private const string ApplicationName = "CodeNav";

        public static T Load<T>(string solutionFilePath)
        {
            try
            {
                if (!File.Exists(solutionFilePath)) return (T)Activator.CreateInstance(typeof(T));

                var solutionFolder = Path.GetDirectoryName(solutionFilePath);
                var settingFilePath = Path.Combine(solutionFolder, $"{ApplicationName}.json");

                if (File.Exists(settingFilePath))
                {
                    var json = File.ReadAllText(settingFilePath);
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception e)
            {
                LogHelper.Log("Error deserializing Solution Storage", e);
            }

            return (T)Activator.CreateInstance(typeof(T));
        }

        public static void Save<T>(string solutionFilePath, T storage)
        {
            if (!File.Exists(solutionFilePath)) return;

            var json = JsonConvert.SerializeObject(storage);

            var solutionFolder = Path.GetDirectoryName(solutionFilePath);
            File.WriteAllText(Path.Combine(solutionFolder, $"{ApplicationName}.json"), json);
        }

        public static void SaveToSolutionStorage(string solutionFilePath, CodeDocumentViewModel codeDocumentViewModel)
        {
            if (string.IsNullOrEmpty(solutionFilePath)) return;

            var solutionStorageModel = SolutionStorageHelper.Load<SolutionStorageModel>(solutionFilePath);

            if (solutionStorageModel.Documents == null)
            {
                solutionStorageModel.Documents = new List<CodeDocumentViewModel>();
            }

            var storageItem = solutionStorageModel.Documents
                .FirstOrDefault(d => d.FilePath.Equals(codeDocumentViewModel.FilePath));
            solutionStorageModel.Documents.Remove(storageItem);

            solutionStorageModel.Documents.Add(codeDocumentViewModel);

            SolutionStorageHelper.Save<SolutionStorageModel>(solutionFilePath, solutionStorageModel);
        }
    }
}
