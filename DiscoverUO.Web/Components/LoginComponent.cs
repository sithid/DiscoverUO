using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

public class LoginComponent : ComponentBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;

    public string Username { get; set; }
    public string Password { get; set; }
    public string ErrorMessage { get; set; }
    public bool ShowErrorMessage { get; set; }

    public LoginComponent(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage, NavigationManager navigationManager)
    {
        _httpClientFactory = httpClientFactory;
        _localStorage = localStorage;
        _navigationManager = navigationManager;
    }

    public async Task HandleLogin()
    {
        ShowErrorMessage = false;

        try
        {
            var client = _httpClientFactory.CreateClient("DiscoverUOApiClient");
            var response = client.PostAsJsonAsync("api/users/Authenticate", new LoginRequest { Username = Username, Password = Password }).Result;
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadAsStringAsync();
            await _localStorage.SetItemAsync("jwtToken", token);

            _navigationManager.NavigateTo("/");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Authentication failed: {ex.Message}";
            ShowErrorMessage = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
            ShowErrorMessage = true;
        }
    }
}