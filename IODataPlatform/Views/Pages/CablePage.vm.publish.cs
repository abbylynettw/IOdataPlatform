namespace IODataPlatform.Views.Pages;

// 发布部分

partial class CableViewModel {

    [RelayCommand]
    private void Publish() {
        navigation.NavigateWithHierarchy(typeof(SubPages.Cable.PublishPage));
    }

}