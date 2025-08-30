using BroPilot.Services;
using BroPilot.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BroPilot
{
    public static class DependencyInjection
    {
        public static ServiceProvider BuildServiceProvider(Action<ServiceCollection> builder = default)
        {
            var services = new ServiceCollection();
            builder?.Invoke(services);
            services.AddHttpClient();
            services
                .AddSingleton<ToolWindow1Control>()
                .AddSingleton<ChatWindow>()
                .AddSingleton<SessionsViewModel>()
                .AddSingleton<AgentsViewModel>()
                .AddSingleton<ChatWindowViewModel>()
                .AddSingleton<OpenAIEndpointService>()
                ;

            return services.BuildServiceProvider();
        }
    }
}
