using DiscoverUO.Api.Models;

namespace DiscoverUO.Api.Data.Repositories.Contracts
{
    public interface IServerDataRepository
    {        
        Task<bool> ServerExists(int id);
        Task<List<Server>> GetServers();
        Task<Server> GetServerById(int id);
        Task<bool> PutServer(int id, Server server);
        Task<Server> PostServer(Server server);
    }
}
