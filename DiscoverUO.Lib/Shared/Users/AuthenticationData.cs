namespace DiscoverUO.Lib.Shared.Users
{
    public class AuthenticationData
    {
        public string? Username { get; set; }
        public string? Password { get; set; }


        public AuthenticationData() : this(string.Empty, string.Empty) { }

        public AuthenticationData(string userName, string password) { Username = userName; Password = password; }
    }
}