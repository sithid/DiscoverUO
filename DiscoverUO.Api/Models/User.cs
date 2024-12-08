using DiscoverUO.Lib.Shared.Users;
namespace DiscoverUO.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? PasswordHash { get; set; }
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public int DailyVotesRemaining { get; set; }
        public UserProfile? Profile { get; set; }
        public UserFavoritesList? Favorites { get; set; }
        public string? CreationDate {  get; set; }
        public bool Banned { get; set; }
    }
}