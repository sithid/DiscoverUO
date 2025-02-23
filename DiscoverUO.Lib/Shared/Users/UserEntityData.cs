namespace DiscoverUO.Lib.Shared.Users
{
    public class UserEntityData
    {
        public string? UserName { get; set; }
        public int DailyVotesRemaining { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int ProfileId { get; set; }
        public int FavoritesId { get; set; }
        public string? CreationDate { get; set; }
        public bool Banned { get; set; }
    }
}