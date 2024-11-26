
namespace DiscoverUO.Lib.Shared.Users
{
    public class AuthenticationData
    {
        public string? Username { get; set; }
        public string? Password { get; set; }

        public AuthenticationData() : this(string.Empty, string.Empty, false) { }

        public AuthenticationData(string userName, string password, bool hashPassword )
        {
            Username = userName;

            if( hashPassword )
                Password = BCrypt.Net.BCrypt.HashPassword(password);
            else
                Password = password;
        }
    }
}