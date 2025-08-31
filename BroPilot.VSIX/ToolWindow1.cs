using BroPilot.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BroPilot.Extension
{
    [Guid("4b51c7e3-6ede-4efc-a43b-c077454dfb61")]
    public class ToolWindow1 : ToolWindowPane
    {
        public ToolWindow1() : base(null)
        {
            this.Caption = "BroPilot";

            var serviceProvider = DependencyInjection.BuildServiceProvider(
                 services => services
                     .AddSingleton<IContextProvider, VisualStudioContextProvider>()
                 );

            var toolWindow1Control = serviceProvider.GetRequiredService<ToolWindow1Control>();
            var sessionsViewModel = serviceProvider.GetRequiredService<SessionsViewModel>();
            var modelsViewModel = serviceProvider.GetRequiredService<ModelsViewModel>();

            sessionsViewModel.LoadSessions();
            modelsViewModel.LoadModels();

            Content = toolWindow1Control;
        }
    }
}
