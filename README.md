# TaskManager
Task manager helps to hide parallel code from developers. Designed by me some time ago, recreated to use in new project.


# Content of the NuGet package

## Producer/Consumer pattern
This nuget package contains thread safe implementation of Producer Consumer Pattern allowing us to produce and consume data in parallel manner.

### AsyncPCCollection\<T> class
Contains implementation of `IAsyncProducer<T>` and `IAsyncConsumer<T>` allowing user to pass them to worker to produce/consume desired values.

```` csharp
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
````

```` csharp
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
````

## Usage
Create new object of `AsyncPCCollection<T>` and fill it with values of `T` after you are done, mark collection as done by calling `FinishAsync()`. This will indicate to consumers that there will be no more data comming and they can finish their work after they are done with the stuff that they already got from producer. 

