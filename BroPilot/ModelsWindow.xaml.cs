using BroPilot.ViewModels;
using System.Windows.Controls;

namespace BroPilot
{
    public partial class ModelsWindow : UserControl
    {
        public ModelsWindow(ModelsViewModel modelsViewModel)
        {
            InitializeComponent();
            DataContext = modelsViewModel;
        }
    }
}
