using BroPilot.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace BroPilot
{
    public partial class ModelsWindow : UserControl
    {
        private readonly ModelsViewModel modelsViewModel;

        public ModelsWindow(ModelsViewModel modelsViewModel)
        {
            InitializeComponent();
            DataContext = modelsViewModel;
            this.modelsViewModel = modelsViewModel;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            modelsViewModel.SaveModels();
        }
    }
}
