namespace TaskManager.Threading
{
    public interface IAsyncConsumer<T>
    {
        Task<T?> GetAsync();
        Task<IEnumerable<T>?> GetAllAsync();
        Task<bool> IsFinishedAndEmptyAsync();
        Task<bool> IsEmptyAsync();
        Task<bool> IsFinishedAsync();
        Task<uint> GetConsumedCountAsync();
    }
}