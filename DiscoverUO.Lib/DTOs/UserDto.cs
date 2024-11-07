namespace DiscoverUO.Lib.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public int DailyVotesRemaining { get; set; }

        public List<int>? ServersAddedIds { get; set; }
        public int? ProfileId { get; set; }
        public int? FavoritesId { get; set; }
    }
}