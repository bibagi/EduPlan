using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Schedule.Core.Models;
using Schedule.Core.Services;

namespace Schedule.Core.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _login = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public User? AuthenticatedUser { get; private set; }

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введите логин и пароль";
            return;
        }

        var user = await _authService.AuthenticateAsync(Login, Password);
        
        if (user == null)
        {
            ErrorMessage = "Неверный логин или пароль";
            return;
        }

        AuthenticatedUser = user;
        ErrorMessage = string.Empty;
    }
}
