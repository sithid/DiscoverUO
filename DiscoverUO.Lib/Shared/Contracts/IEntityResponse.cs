namespace DiscoverUO.Lib.Shared.Contracts
{
    public interface IEntityResponse<T> : IResponse
    {
        public T Entity { get; set; }
    }
}
