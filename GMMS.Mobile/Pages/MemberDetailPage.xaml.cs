using GMMS.Mobile.ViewModels;

namespace GMMS.Mobile.Pages;

[QueryProperty(nameof(MemberId), "Id")]
public partial class MemberDetailPage : ContentPage
{
    private readonly MemberViewModel _viewModel;
    private int _memberId;

    public string MemberId
    {
        set => _memberId = int.TryParse(value, out var id) ? id : 0;
    }

    public MemberDetailPage(MemberViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_memberId > 0)
        {
            await _viewModel.LoadMemberAsync(_memberId);
        }
    }
}
