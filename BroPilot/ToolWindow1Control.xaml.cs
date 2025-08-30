using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace BroPilot
{
    public partial class ToolWindow1Control : UserControl
    {
        public ToolWindow1Control(IHttpClientFactory httpClientFactory, ChatWindow chatWindow)
        {
            //ApplicationThemeManager.Apply(this);
            this.InitializeComponent();
            this.Content = chatWindow;
        }

        private void FlowDocumentScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = FindParent<ScrollViewer>(sender as DependencyObject);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent) 
                    return parent;

                child = VisualTreeHelper.GetParent(child);
            }

            return null;
        }
    }
}