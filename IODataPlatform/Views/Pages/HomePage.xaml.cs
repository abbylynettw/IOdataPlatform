using IODataPlatform.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
                var scaleTransform = border.RenderTransform as ScaleTransform;
                if (scaleTransform != null)
                {
                    scaleTransform.ScaleX = 1.05;
                    scaleTransform.ScaleY = 1.05;
                }
            }
        }

        private void Card_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                var scaleTransform = border.RenderTransform as ScaleTransform;
                if (scaleTransform != null)
                {
                    scaleTransform.ScaleX = 1.0;
                    scaleTransform.ScaleY = 1.0;
                }
            }
        }
    }
}
