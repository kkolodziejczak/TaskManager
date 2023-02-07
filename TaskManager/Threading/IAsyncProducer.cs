namespace TaskManager.Threading
{
    public interface IAsyncProducer<T>
    {
        Task ProduceAsync(T item, CancellationToken token);
        Task ProduceAsync(IEnumerable<T> items, CancellationToken token);
        Task FinishAsync();
        Task<uint> GetProducedCountAsync();
    }
}