using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Views.Pages;
using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.SubPages.AQJ
{
    /// <summary>
    /// ConfigSignalGroupDetailPage.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigSignalGroupDetailPage : Page, INavigableView<ConfigSignalGroupDetailViewModel>
    {
        public ConfigSignalGroupDetailViewModel ViewModel { get; }

        public ConfigSignalGroupDetailPage(ConfigSignalGroupDetailViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
