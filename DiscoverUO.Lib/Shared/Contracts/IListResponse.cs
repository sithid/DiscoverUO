namespace DiscoverUO.Lib.Shared.Contracts
{
    public interface IListResponse<T> : IResponse
    {
        public List<T> List { get; set; }
    }
}
