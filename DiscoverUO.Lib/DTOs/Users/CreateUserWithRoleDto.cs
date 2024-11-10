namespace DiscoverUO.Lib.DTOs.Users
{
    public class CreateUserWithRoleDto
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public UserRole Role { get; set; }
    }
}
