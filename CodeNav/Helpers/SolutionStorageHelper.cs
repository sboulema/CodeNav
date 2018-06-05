using Newtonsoft.Json;
using System;
using System.IO;

namespace CodeNav.Helpers
{
    public static class SolutionStorageHelper
    {
        private const string ApplicationName = "CodeNav";

        public static T Load<T>(string solutionFilePath)
        {
            if (!File.Exists(solutionFilePath)) return (T)Activator.CreateInstance(typeof(T));

            var solutionFolder = Path.GetDirectoryName(solutionFilePath);
            var settingFilePath = Path.Combine(solutionFolder, $"{ApplicationName}.json");

            if (File.Exists(settingFilePath))
            {
                var json = File.ReadAllText(settingFilePath);
                return JsonConvert.DeserializeObject<T>(json);
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
    }
}
