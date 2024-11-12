using System.Text.Json.Serialization;

public class LoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }

    
    public LoginRequest() : this(string.Empty, string.Empty)
    {
    }

    public LoginRequest(string userName, string password)
    { 
        Username = userName;
        Password = password; 
    }
}