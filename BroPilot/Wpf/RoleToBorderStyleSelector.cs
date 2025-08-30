using BroPilot.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace BroPilot.Wpf
{
    public class RoleToBorderStyleSelector : StyleSelector
    {
        public Style ChatStyle { get; set; }
        public Style UserStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var role = (item as MessageViewModel)?.Role;
            return role == "User" ? UserStyle : ChatStyle;
        }
    }
}
