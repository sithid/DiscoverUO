namespace DiscoverUO.Lib.Shared.Users
{
    public class UpdateUserPasswordRequest
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
