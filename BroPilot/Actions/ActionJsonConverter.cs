using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BroPilot.Actions
{
    public class ActionJsonConverter : JsonConverter<BaseAction>
    {
        public override BaseAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var jsonDocument = JsonDocument.ParseValue(ref reader))
            {
                var rootElement = jsonDocument.RootElement;

                if (!rootElement.TryGetProperty("action", out var actionProp))
                {
                    throw new JsonException("Missing 'action' property");
                }

                var action = actionProp.GetString() ?? throw new JsonException("'action' is null");

                BaseAction result;

                switch (action)
                {
                    case "addmethod":
                        result = JsonSerializer.Deserialize<AddMethodAction>(rootElement.GetRawText(), options);
                        break;

                    case "replacemethod":
                        result = JsonSerializer.Deserialize<ReplaceMethodAction>(rootElement.GetRawText(), options);
                        break;

                    case "replaceclass":
                        result = JsonSerializer.Deserialize<ReplaceClassAction>(rootElement.GetRawText(), options);
                        break;

                    case "addclass":
                        result = JsonSerializer.Deserialize<AddClassAction>(rootElement.GetRawText(), options);
                        break;

                    default:
                        throw new JsonException("Unknown action type: " + action);
                }

                return result;
            }
        }

        public override void Write(Utf8JsonWriter writer, BaseAction value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
        }
    }
}
