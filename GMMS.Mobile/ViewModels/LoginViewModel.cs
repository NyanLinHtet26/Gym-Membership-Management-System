using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMMS.Mobile.Models;
using GMMS.Mobile.Services;
using GMMS.Mobile.Storage;

namespace GMMS.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthApiService _authApiService;
    private readonly TokenStorage _tokenStorage;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    public LoginViewModel(AuthApiService authApiService, TokenStorage tokenStorage)
    {
        _authApiService = authApiService;
        _tokenStorage = tokenStorage;
    }

    partial void OnUserNameChanged(string value) => LoginCommand.NotifyCanExecuteChanged();

    partial void OnPasswordChanged(string value) => LoginCommand.NotifyCanExecuteChanged();

    private bool CanLogin()
        => !IsBusy && !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _authApiService.LoginAsync(new LoginRequest
            {
                UserName = UserName.Trim(),
                Password = Password
            });

            if (result is null)
            {
                return;
            }

            await _tokenStorage.SaveAuthAsync(result.AccessToken, result.RefreshToken.Token, result.User);

            if (result.User.MustChangePassword)
            {
                await Shell.Current.DisplayAlert(
                    "Password Change Required",
                    "Please ask the gym owner to reset your password.",
                    "OK");
            }

            await Shell.Current.GoToAsync("//member-list");
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.ToString(); 
        }
        finally
        {
            IsBusy = false;
        }
    }
}
