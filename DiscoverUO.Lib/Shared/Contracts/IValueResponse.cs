namespace DiscoverUO.Lib.Shared.Contracts
{
    public interface IValueResponse<T> : IResponse
    {
        public T Value { get; set; }
    }
}
