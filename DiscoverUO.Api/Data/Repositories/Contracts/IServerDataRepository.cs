using DiscoverUO.Api.Models;

namespace DiscoverUO.Api.Data.Repositories.Contracts
{
    public interface IServerDataRepository
    {        
        Task<List<Server>> GetServers();
        Task<Server> GetServer( int id );
        Task<bool> PutServer( Server server );
        Task<bool> PostServer(  Server server );
        Task<bool> DeleteServer( int id );
        Task<bool> DeleteServer( Server server );
        Task<bool> ServerExists( int id );
    }
}
