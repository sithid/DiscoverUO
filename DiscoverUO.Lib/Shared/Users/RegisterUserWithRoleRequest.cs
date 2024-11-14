namespace DiscoverUO.Lib.Shared.Users
{
    public class RegisterUserWithRoleRequest
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }  
        public UserRole Role { get; set; }
    }
}
