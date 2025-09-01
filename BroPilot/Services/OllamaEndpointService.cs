using BroPilot.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BroPilot.Services
{
    public class OllamaEndpointService : IEndpointService
    {
        private HttpClient httpClient;

        public OllamaEndpointService(IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient();
        }

        public Task<string> ChatCompletionStream(Model agent, Message[] messages, Action<string> handler = null)
        {
            return Task.Run(async () =>
            {
                var jsonPayload = JsonSerializer.Serialize(new
                {
                    model = agent.ModelName,
                    messages,
                    stream = true,
                });

                var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, agent.Address + "/api/chat")
                {
                    Content = requestContent
                };

                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();
                var reader = new StreamReader(stream);
                string fullContent = "";

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var node = JsonNode.Parse(line);

                    bool done = node?["done"]?.GetValue<bool>() ?? false;
                    if (done)
                        break;

                    var delta = node?["message"]?["content"]?.ToString();
                    if (!string.IsNullOrEmpty(delta))
                    {
                        handler?.Invoke(delta);
                        fullContent += delta;
                    }
                }

                return fullContent;
            });
        }

        public Task<(Message message, int tokenCount)> ChatCompletion(Model agent, Message[] messages)
        {
            return Task.Run(async () =>
            {
                var jsonPayload = JsonSerializer.Serialize(new
                {
                    model = agent.ModelName,
                    messages,
                    stream = false,
                });

                var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(agent.Address + "/api/chat", requestContent);
                var responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                var node = JsonNode.Parse(responseBody);

                var messageContent = node?["message"]?["content"]?.ToString() ?? string.Empty;

                if (messageContent.Contains("<think>"))
                {
                    messageContent = Regex.Replace(messageContent, @"<think>[\s\S]*?</think>\s*", "").Trim();
                }

                var promptTokens = node?["message"]?["prompt_eval_count"]?.GetValue<int>() ?? 0;
                var responseTokens = node?["message"]?["eval_count"]?.GetValue<int>() ?? 0;
                int tokenCount = promptTokens + responseTokens;

                var returnMessage = new Message { role = "assistant", content = messageContent };
                return (returnMessage, tokenCount);
            });
        }
    }
}
