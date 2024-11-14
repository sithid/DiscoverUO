using DiscoverUO.Lib.DTOs.Users;
using System.Net;


namespace DiscoverUO.Lib.Shared.Contracts
{
    public interface IDataResponse<T> : IResponse
    {
        public T Data { get; set; }
    }
}
