namespace DiscoverUO.Lib.Shared.Users
{
    public class UpdateUserPasswordData
    {
        public bool PasswordPreHashed { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
