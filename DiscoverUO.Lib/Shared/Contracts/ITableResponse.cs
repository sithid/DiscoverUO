namespace DiscoverUO.Lib.Shared.Contracts
{
    public interface IListResponse<T> : IResponse
    {
        public List<T> Table { get; set; }
    }
}
