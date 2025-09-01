using System;
using static BroPilot.Models.Model;

namespace BroPilot.Models
{
    public class Model
    {
        public string Name { get; set; }

        public string ModelName { get; set; }

        public int Temperature { get; set; }

        public string Address { get; set; }

        public int MaxTokens { get; set; }

        public ApiType Type { get; set; }

        public enum ApiType
        {
            OpenAI,
            Ollama
        }
    }
    public static class ApiTypeEnumValues
    {
        public static Array Values => Enum.GetValues(typeof(ApiType));
    }
}
