using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BroPilot.Actions
{
    public class ActionContainer
    {
        [JsonPropertyName("actions")]
        public List<BaseAction> Actions { get; set; } = new List<BaseAction>();
    }
}
