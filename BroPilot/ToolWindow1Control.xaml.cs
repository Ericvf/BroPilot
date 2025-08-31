using BroPilot.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Controls;

namespace BroPilot
{
    public partial class ToolWindow1Control : UserControl
    {
        public ToolWindow1Control(ToolWindowState state)
        {
            this.InitializeComponent();
            this.DataContext = state;
            state.ShowChatWindow();
        }
    }

    public class ToolWindowState : BaseViewModel
    {
        public ToolWindowState(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private object contentControl;
        private readonly IServiceProvider serviceProvider;

        public object ContentControl
        {
            get => contentControl;
            set
            {
                if (contentControl != value)
                {
                    contentControl = value;
                    OnPropertyChanged(nameof(ContentControl));
                }
            }
        }

        public void ShowChatWindow()
        {
            var window = serviceProvider.GetRequiredService<ChatWindow>();
            ContentControl = window;
        }

        internal void ShowSessionsWindow()
        {
            var window = serviceProvider.GetRequiredService<SessionsWindow>();
            ContentControl = window;
        }
    }
}