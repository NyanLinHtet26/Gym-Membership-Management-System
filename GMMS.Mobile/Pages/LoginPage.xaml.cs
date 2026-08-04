using GMMS.Mobile.ViewModels;

namespace GMMS.Mobile.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private async void OnPasswordCompleted(object? sender, EventArgs e)
    {
        if (_viewModel.LoginCommand.CanExecute(null))
        {
            await _viewModel.LoginCommand.ExecuteAsync(null);
        }
    }
}
