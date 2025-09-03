using System.Text.Json.Serialization;

namespace BroPilot.Actions
{
    public class ReplaceMethodAction : BaseAction
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        public override string Summary => $"Replaced method {Method} in {Classname} 🎉";

    }
}
