using BroPilot.ViewModels;
using System.Windows.Controls;

namespace BroPilot
{
    public partial class ChatWindow : UserControl
    {
        public ChatWindow(ChatWindowViewModel chatWindowViewModel)
        {
            InitializeComponent();
            DataContext = chatWindowViewModel;
        }
    }
}
