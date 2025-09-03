using BroPilot.Models;
using System;
using System.Threading.Tasks;

namespace BroPilot.Services
{
    public interface IEndpointService
    {
        Task<string> ChatCompletionStream(Model agent, Message[] messages, Action<string> handler = null);

        Task<(Message message, int tokenCount)> ChatCompletion(Model agent, Message[] messages, object schema = null);
    }
}
