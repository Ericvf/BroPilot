using System.Text.Json.Serialization;

namespace BroPilot.Actions
{
    public class ReplaceClassAction : BaseAction
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;


        public override string Summary => $"Replaced class {Classname} 🎉";
    }
}
