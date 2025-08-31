using BroPilot.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BroPilot.Services
{
    public class OpenAIEndpointService
    {
        private HttpClient httpClient;

        public OpenAIEndpointService(IHttpClientFactory httpClientFactory)
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
                    temperature = agent.Temperature,
                    max_tokens = -1,
                    stream = true,
                    // response_format = new { type = "json_schema" },
                    // response_format = new
                    // {
                    //     type = "json_schema",
                    //     json_schema = schema
                    // }
                });

                var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, agent.Address + "/v1/chat/completions")
                {
                    Content = requestContent
                };

                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();
                var reader = new StreamReader(stream);

                string fullContent = "";
                string fullJson = "";
                string buffer = "";

                while (!reader.EndOfStream)
                {
                    char[] charBuffer = new char[256];
                    int read = await reader.ReadAsync(charBuffer, 0, charBuffer.Length);
                    if (read == 0) break;

                    buffer += new string(charBuffer, 0, read);
                    fullJson += buffer;

                    int lineEnd;
                    while ((lineEnd = buffer.IndexOf('\n')) >= 0)
                    {
                        string line = buffer.Substring(0, lineEnd).TrimEnd('\r');
                        buffer = buffer.Substring(lineEnd + 1);

                        if (line.StartsWith("data: "))
                        {
                            var json = line.Substring("data: ".Length).Trim();
                            if (json == "[DONE]")
                                break;

                            var node = JsonNode.Parse(json);
                            var delta = node?["choices"]?[0]?["delta"]?["content"]?.ToString();
                            if (!string.IsNullOrEmpty(delta))
                            {
                                handler?.Invoke(delta.ToString());
                                fullContent += delta;
                            }
                        }
                    }
                }

                return fullContent;
            });
        }

        public async Task<(Message message, int tokenCount)> ChatCompletion(Model agent, Message[] messages)
        {
            var x = JsonSerializer.Serialize(new
            {
                model = agent.ModelName,
                messages,
                temperature = agent.Temperature,
                max_tokens = -1,
                stream = false,
                // response_format = new { type = "json_schema" },
                // response_format = new
                // {
                //     type = "json_schema",
                //     json_schema = schema
                // }
            });

            var requestContent = new StringContent(x, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(agent.Address + "/v1/chat/completions", requestContent);
            var responseBody = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var node = JsonNode.Parse(responseBody);
            var messageContent = node?["choices"]?[0]?["message"]?["content"]?.ToString() ?? string.Empty;
            var tokens = node?["usage"]?["total_tokens"]?.ToString() ?? string.Empty;

            if (messageContent?.Contains("<think>") == true)
            {
                messageContent = Regex.Replace(messageContent, @"<think>[\s\S]*?</think>\s*", "").Trim();
            }

            var returnMessage = new Message { role = "assistant", content = messageContent };
            return (returnMessage, Convert.ToInt32(tokens));
        }
    }
}
