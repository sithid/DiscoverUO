using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DiscoverUO.Api.Models;

namespace DiscoverUO.Api.Data
{
    public class DiscoverUODatabaseContext : DbContext
    {
        public DbSet<Server> Servers { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;

        public DiscoverUODatabaseContext (DbContextOptions<DiscoverUODatabaseContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
