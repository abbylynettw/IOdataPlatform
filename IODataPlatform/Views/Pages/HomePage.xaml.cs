using IODataPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IODataPlatform.Views.Pages
{
    public partial class HomePage : INavigableView<HomeViewModel>
    {

        public HomeViewModel ViewModel { get; }

        public HomePage(HomeViewModel viewModel, GlobalModel model)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void Card_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Effect = new DropShadowEffect
                {
                    Color = ((LinearGradientBrush)border.Background).GradientStops[1].Color,
                    Direction = 270,
                    BlurRadius = 15,
                    ShadowDepth = 8
                };
            }
        }

        private void Card_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Effect = new DropShadowEffect
                {
                    Color = ((LinearGradientBrush)border.Background).GradientStops[1].Color,
                    Direction = 270,
                    BlurRadius = 10,
                    ShadowDepth = 5
                };
            }
        }
    }
}
