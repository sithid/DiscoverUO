namespace DiscoverUO.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public int FavoritesListId { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? DailyVotesRemaining { get; set; }
    }
}
