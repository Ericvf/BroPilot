using System.Text.Json.Serialization;

namespace BroPilot.Actions
{
    public class AddClassAction : BaseAction
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        public override string Summary => "Added class 🎉";
    }
}
