using DiscoverUO.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscoverUO.Api.Data
{
    public class ApiDbContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ServerListing> Servers { get; set; }
        public DbSet<FavoritesList> Favorites { get; set; }

        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base( options )
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
}
