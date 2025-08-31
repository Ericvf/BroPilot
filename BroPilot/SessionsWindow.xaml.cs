using BroPilot.ViewModels;
using System.Windows.Controls;

namespace BroPilot
{
    public partial class SessionsWindow : UserControl
    {
        public SessionsWindow(SessionsViewModel sessionsViewModel)
        {
            InitializeComponent();
            this.DataContext = sessionsViewModel;
        }
    }
}
