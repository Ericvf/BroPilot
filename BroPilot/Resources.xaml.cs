using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BroPilot
{
    public partial class Resources {

        private void FlowDocumentScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // find the first parent ScrollViewer
            var parentScroll = FindParent<ScrollViewer>(sender as DependencyObject);
            if (parentScroll != null)
            {
                parentScroll.ScrollToVerticalOffset(parentScroll.VerticalOffset - e.Delta);
                e.Handled = true; // mark handled so it doesn’t scroll internally
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent) return parent;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }
    }
}