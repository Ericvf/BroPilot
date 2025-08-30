using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace BroPilot.Extension
{
    [Guid("4b51c7e3-6ede-4efc-a43b-c077454dfb61")]
    public class ToolWindow1 : ToolWindowPane
    {
        public ToolWindow1() : base(null)
        {
            this.Caption = "BroPilot";
            var serviceProvider = DependencyInjection.BuildServiceProvider();
            var toolWindow1Control = serviceProvider.GetRequiredService<ToolWindow1Control>();
            this.Content = toolWindow1Control;
        }
    }
}
