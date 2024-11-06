using DiscoverUO.Api.Data.Repositories.Contracts;
using DiscoverUO.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DiscoverUO.Api.Data.Repositories
{
    public class ServerDataRepository : IServerDataRepository
    {
        private readonly DiscoverUODatabaseContext _context;

        public ServerDataRepository( DiscoverUODatabaseContext context )
        {
            _context = context;
        }

        public async Task<List<Server>> GetServers()
        {
            var serverList = await _context.Servers.ToListAsync();

            return serverList;
        }

        public async Task<Server> GetServerById( int id )
        {
            var server = await _context.Servers.FirstOrDefaultAsync( server => server.Id == id );

            return server;
        }

        public async Task<bool> PutServer( int id, Server server )
        {
            _context.Entry(server).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch ( Exception ex )
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }
        public async Task<Server> PostServer( Server server )
        {
            _context.AddAsync( server );
            await _context.SaveChangesAsync();

            return server;
        }

        public async Task<bool> ServerExists(int id)
        {
            return _context.Servers.Any( server => server.Id == id);
        }
    }
}
