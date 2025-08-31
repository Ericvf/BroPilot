using BroPilot.Models;
using System;
using System.IO;
using System.Text.Json;

namespace BroPilot.ViewModels
{
    public class ModelsManager : IModelsManager
    {
        public Model[] LoadModels()
        {
            string path = GetBasePath();
            var fileName = Path.Combine(path, "models.json");

            if (File.Exists(fileName))
            {
                try
                {
                    var content = File.ReadAllText(fileName);
                    var models = JsonSerializer.Deserialize<Model[]>(content);
                    return models;
                }
                catch
                {
                }
            }

            return new[] {
            new Model () {
                Address = "http://localhost:1234",
                ModelName = "qwen/qwen3-8b",
                Name = "qwen/qwen3-8b",
                Temperature = 0,
                MaxTokens = 8000
            }};
        }

        public void SaveModels(Model[] models)
        {
            string path = GetBasePath();

            var options = new JsonSerializerOptions { WriteIndented = true };
            var content = JsonSerializer.Serialize(models, options);

            Directory.CreateDirectory(path);

            var fileName = Path.Combine(path, "models.json");
            File.WriteAllText(fileName, content);
        }

        private static string GetBasePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BroPilot");
        }
    }
}
