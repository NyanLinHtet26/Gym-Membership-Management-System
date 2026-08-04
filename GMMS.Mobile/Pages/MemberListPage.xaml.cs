using GMMS.Mobile.ViewModels;

namespace GMMS.Mobile.Pages;

public partial class MemberListPage : ContentPage
{
    private readonly MemberViewModel _viewModel;

    public MemberListPage(MemberViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
