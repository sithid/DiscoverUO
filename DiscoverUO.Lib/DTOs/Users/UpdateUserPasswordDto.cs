namespace DiscoverUO.Lib.DTOs.Users
{
    public class UpdateUserPasswordDto
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
