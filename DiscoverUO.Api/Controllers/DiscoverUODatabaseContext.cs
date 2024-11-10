using DiscoverUO.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscoverUO.Api.Controllers
{
    public class DiscoverUODatabaseContext : DbContext
    {
        public DbSet<Server> Servers { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<UserProfile> UserProfiles { get; set; } = default!;
        public DbSet<UserFavoritesList> UserFavoritesLists { get; set; } = default!;
        public DbSet<UserFavoritesListItem> UserFavoritesListItems { get; set; } = default!;

        public DiscoverUODatabaseContext(DbContextOptions<DiscoverUODatabaseContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
