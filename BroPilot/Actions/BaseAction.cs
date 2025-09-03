using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BroPilot.Actions
{

    [JsonConverter(typeof(ActionJsonConverter))]
    public abstract class BaseAction
    {
        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;


        [JsonPropertyName("file")]
        public string File { get; set; } = string.Empty;


        [JsonPropertyName("classname")]
        public string Classname { get; set; } = string.Empty;


        [JsonPropertyName("motivation")]
        public string Motivation { get; set; } = string.Empty;


        [JsonIgnore]
        public abstract string Summary { get; }
    }
}
