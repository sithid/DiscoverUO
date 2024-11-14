namespace DiscoverUO.Lib.Shared.Users
{
    public class AuthenticationRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }


        public AuthenticationRequest() : this(string.Empty, string.Empty) { }

        public AuthenticationRequest(string userName, string password) { Username = userName; Password = password; }
    }
}