namespace TaskManager.Threading
{
    public interface IAsyncConsumer<T>
    {
        Task<T?> GetAsync();
        Task<IEnumerable<T>?> GetAllAsync();
        Task<bool> IsFinishedAndEmptyAsync(CancellationToken token);
        Task<uint> GetConsumedCountAsync();
    }
}