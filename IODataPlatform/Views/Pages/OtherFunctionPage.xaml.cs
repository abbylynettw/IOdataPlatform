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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IODataPlatform.Views.Pages
{
    /// <summary>
    /// OtherFunctionPage.xaml 的交互逻辑
    /// </summary>
    public partial class OtherFunctionPage  : INavigableView<OtherFunctionViewModel> 
    {        

        public OtherFunctionViewModel ViewModel { get; }

        public OtherFunctionPage(OtherFunctionViewModel viewModel, GlobalModel model)
        {
            ViewModel = viewModel;            
            DataContext = this;

            InitializeComponent();
        }
    }
}
