using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMMS.Mobile.Models;
using GMMS.Mobile.Services;
using GMMS.Mobile.Storage;

namespace GMMS.Mobile.ViewModels;

public partial class MemberViewModel : ObservableObject
{
    private readonly MemberApiService _memberApiService;
    private readonly AuthApiService _authApiService;
    private readonly TokenStorage _tokenStorage;
    private bool _initialized;
    private int _lastMemberId;

    public ObservableCollection<Member> Members { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? currentUserName;

    [ObservableProperty]
    private Member? detail;

    public MemberViewModel(MemberApiService memberApiService, AuthApiService authApiService, TokenStorage tokenStorage)
    {
        _memberApiService = memberApiService;
        _authApiService = authApiService;
        _tokenStorage = tokenStorage;
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await LoadCurrentUserAsync();
        await LoadMembersAsync();
    }

    [RelayCommand]
    private async Task LoadMembersAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _memberApiService.GetMemberListAsync(pageNumber: 1, pageSize: 100);
            if (result is null)
            {
                return;
            }

            Members.Clear();
            foreach (var member in result.Members)
            {
                Members.Add(member);
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            ErrorMessage = "Unable to load members. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task RefreshMembersAsync() => LoadMembersAsync();

    [RelayCommand]
    private async Task OpenMemberAsync(Member member)
    {
        if (member is null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"member-detail?Id={member.MemberId}");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        try
        {
            await _authApiService.LogoutAsync();
        }
        catch
        {
        }

        await _tokenStorage.ClearAsync();
        await Shell.Current.GoToAsync("//login");
    }

    public async Task LoadMemberAsync(int memberId)
    {
        _lastMemberId = memberId;
        IsBusy = true;
        ErrorMessage = null;
        Detail = null;

        try
        {
            Detail = await _memberApiService.GetMemberAsync(memberId);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            ErrorMessage = "Unable to load member details. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task RetryDetailAsync()
        => _lastMemberId > 0 ? LoadMemberAsync(_lastMemberId) : Task.CompletedTask;

    private async Task LoadCurrentUserAsync()
    {
        var user = await _tokenStorage.GetUserAsync();
        CurrentUserName = user is null ? null : $"{user.UserName} ({user.Role})";
    }
}
