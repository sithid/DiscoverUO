using DiscoverUO.Api.Data.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace DiscoverUO.Api.Data.Repositories
{
    public class ServerDataRepository : IServerDataRepository
    {
        private readonly DiscoverUODatabaseContext _context;

        public ServerDataRepository( DiscoverUODatabaseContext context )
        {
            _context = context;
        }
    }
}
