using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Views.Pages
{
    public partial class DepXT2ViewModel
    {
        [ObservableProperty]
        private bool canEdit = false;//只有切换子项时候，该权限才会变化

        private void SetPermissionBySubProject()
        {
            if (SubProject is null) { return; }
            CanEdit = SubProject.CreatorUserId == model.User.Id;
        }
    }
}
