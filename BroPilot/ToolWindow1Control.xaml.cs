using System.Net.Http;
using System.Windows.Controls;

namespace BroPilot
{
    public partial class ToolWindow1Control : UserControl
    {
        public ToolWindow1Control(IHttpClientFactory httpClientFactory, ChatWindow chatWindow)
        {
            this.InitializeComponent();
            this.Content = chatWindow;
        }
    }
}