using IODataPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Views.Pages
{
    public partial class CableViewModel
    {
        [RelayCommand]
        private async void CalcCable()
        {
            var list = AllData.ToList();
            AllData = [.. await DataConverter.NumberMatchCable1(list, context)];
        }

    }
}
