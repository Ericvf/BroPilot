using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO.Packaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BroPilot.Extension
{
    [Guid("4b51c7e3-6ede-4efc-a43b-c077454dfb61")]
    public class ToolWindow1 : ToolWindowPane
    {
        public ToolWindow1() : base(null)
        {
            this.Caption = "BroPilot";

            var serviceProvider2 = DependencyInjection.BuildServiceProvider(
                 services => services
                     .AddSingleton<IContextProvider, VisualStudioContextProvider>()
                 );

            var toolWindow1Control = serviceProvider2.GetRequiredService<ToolWindow1Control>();
            Content = toolWindow1Control;
        }

        //public async Task InitializeAsync(AsyncPackage package)
        //{
        //    await package.JoinableTaskFactory.SwitchToMainThreadAsync();

        //    var dte = (EnvDTE.DTE)await package.GetServiceAsync(typeof(SDTE));

        //}

    }
}
