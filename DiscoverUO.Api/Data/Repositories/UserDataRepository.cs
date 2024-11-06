using DiscoverUO.Api.Data.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace DiscoverUO.Api.Data.Repositories
{
    public class UserDataRepository : IUserDataRepository
    {
        private readonly DiscoverUODatabaseContext _context;

        public UserDataRepository(DiscoverUODatabaseContext context)
        {
            _context = _context;
        }
    }
}
