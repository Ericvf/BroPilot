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
                .AddSingleton<ChatWindowViewModel>()
                .AddSingleton<ModelsWindow>()
                .AddSingleton<ModelsViewModel>()
                .AddSingleton<SessionsWindow>()
                .AddSingleton<SessionsViewModel>()
                .AddSingleton<ISessionManager, SessionManager>()
                .AddSingleton<IModelsManager, ModelsManager>()
                .AddSingleton<OpenAIEndpointService>()
                .AddSingleton<OllamaEndpointService>()
                .AddSingleton<ToolWindowState>()
                .AddSingleton<ResourceHelper>()
                .AddSingleton<AnalyzeCodeActionService>()

                ;

            return services.BuildServiceProvider();
        }
    }
}
