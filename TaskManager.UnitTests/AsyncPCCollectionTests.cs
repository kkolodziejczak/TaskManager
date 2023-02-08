using TaskManager.Threading;
using Xunit;

namespace TaskManager.UnitTests;

public class AsyncPCCollectionTests
{
    [Fact]
    public async Task ProduceAsync_Should_Add_NewItem_To_CollectionAsync()
    {
        // Arrange
        var collection = new AsyncPCCollection<string>();

        // Act
        await collection.ProduceAsync("http://google.com/", new CancellationToken());

        // Assert
        Assert.Equal((uint)1, await collection.GetProducedCountAsync());
    }

    [Fact]
    public async Task GetAsync_Should_Return_Item_From_CollectionAsync()
    {
        // Arrange
        var collection = new AsyncPCCollection<string>();
        var itemKey = "http://google.com/";
        await collection.ProduceAsync(itemKey, new CancellationToken());

        // Act
        var item = await collection.GetAsync();

        // Assert
        Assert.Equal("http://google.com/", item);
        Assert.Equal((uint)1, await collection.GetConsumedCountAsync());
    }

    [Fact]
    public async Task GetAsync_Should_Return_Null_If_CollectionWasEmptyAsync()
    {
        // Arrange
        var collection = new AsyncPCCollection<string>();
        var itemKey = "http://google.com/";
        await collection.ProduceAsync(itemKey, new CancellationToken());
        await collection.GetAsync();

        // Act
        var item = await collection.GetAsync();

        // Assert
        Assert.Null(item);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Items_In_FIFO_OrderAsync()
    {
        // Arrange
        var collection = new AsyncPCCollection<string>();
        var itemKey = "http://google.com/";
        await collection.ProduceAsync(itemKey + "0", new CancellationToken());
        await collection.ProduceAsync(itemKey + "1", new CancellationToken());

        // Assert
        Assert.Equal(itemKey + "0", await collection.GetAsync());
        Assert.Equal(itemKey + "1", await collection.GetAsync());
        Assert.Equal((uint)2, await collection.GetConsumedCountAsync());
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_AllItems_In_FIFO_OrderAsync()
    {
        // Arrange
        var collection = new AsyncPCCollection<string>();
        var itemKey = "http://google.com/";
        await collection.ProduceAsync(itemKey + "0", new CancellationToken());
        await collection.ProduceAsync(itemKey + "1", new CancellationToken());
        await collection.FinishAsync();
        
        // Act
        var items = await collection.GetAllAsync();

        // Assert
        var itemsInList = items.ToList();
        Assert.Equal(2, itemsInList.Count);
        Assert.Equal(itemKey + "0", itemsInList[0]);
        Assert.Equal(itemKey + "1", itemsInList[1]);
        Assert.Equal((uint)2, await collection.GetConsumedCountAsync());
        Assert.Equal((uint)2, await collection.GetProducedCountAsync());
        Assert.True(await collection.IsEmptyAsync(), "After getting all items, collection should be empty.");
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Null_When_Collection_IsEmptyAsync()
    {
        // Arrange
        var collection = new AsyncPCCollection<string>();
        var itemKey = "http://google.com/";
        await collection.ProduceAsync(itemKey + "0", new CancellationToken());
        await collection.ProduceAsync(itemKey + "1", new CancellationToken());
        await collection.FinishAsync();
        Assert.False(await collection.IsEmptyAsync(), "After producing new items collection should not be empty.");
        await collection.GetAllAsync();
        
        // Act
        var items = await collection.GetAllAsync();

        // Assert
        Assert.True(await collection.IsEmptyAsync(), "After getting all items, collection should be empty.");
        Assert.Null(items);
    }

    [Fact]
    public async Task AsyncPCCollection_Should_BeFinished_After_Creating_FromCollectionAsync()
    {
        // Arrange
        var collection = new AsyncPCCollection<string>(new[] { "A", "B", "C" });

        // Assert
        Assert.True(await collection.IsFinishedAsync(),
            "Collection should be finished after it was initially created from collection.");
    }

    [Fact]
    public async Task FinalizeAsync_Should_FinalizeCollectionAsync()
    {
        // Arrange
        var collection = new AsyncPCCollection<string>();

        // Act
        await collection.FinishAsync();

        // Assert
        Assert.True(await collection.IsFinishedAsync(), "Collection should be finished after FinishAsync was called.");
    }

    [Fact]
    public async Task GetAsync_Should_Return_Item_From_Collection2Async()
    {
        // Arrange
        var collection = new AsyncPCCollection<string>();
        var itemKey = "http://google.com/";
        await collection.ProduceAsync(itemKey, new CancellationToken());
        await collection.FinishAsync();
        
        // Act
        var item = await collection.GetAsync();

        // Assert
        Assert.Equal("http://google.com/", item);
        Assert.Equal((uint)1, await collection.GetConsumedCountAsync());
    }

    [Fact]
    public async Task ProduceAsync_Should_Produce_AllItemsPassedAsync()
    {
        // Arrange
        var collection = new AsyncPCCollection<string>();
        var items = new [] {"http://google.com/0", "http://google.com/1"};
        
        // Act
        await collection.ProduceAsync(items, new CancellationToken());

        // Assert
        Assert.False(await collection.IsEmptyAsync(), "After producing items collection should not be empty.");
        Assert.Equal((uint)2, await collection.GetProducedCountAsync());
    }

    [Fact]
    public async Task IsFinishedAndEmptyAsync_Should_Return_True_When_Collection_IsEmptyAndFinishedAsync()
    {
        // Arrange
        var collection = new AsyncPCCollection<string>();
        var items = new [] {"http://google.com/0", "http://google.com/1"};
        await collection.ProduceAsync(items, new CancellationToken());
        await collection.FinishAsync();
        await collection.GetAllAsync();

        // Assert
        Assert.True(await collection.IsFinishedAndEmptyAsync(), "After getting all items collection should be empty.");
        Assert.Equal((uint)2, await collection.GetProducedCountAsync());
        Assert.Equal((uint)2, await collection.GetConsumedCountAsync());
    }

    [Fact]
    public async Task IsFinishedAndEmptyAsync_Should_Return_False_When_Collection_IsEmptyAndNotFinishedAsync()
    {
        // Arrange
        var collection = new AsyncPCCollection<string>();
        var items = new [] {"http://google.com/0", "http://google.com/1"};
        await collection.ProduceAsync(items, new CancellationToken());
        //await collection.FinishAsync();
        await collection.GetAllAsync();

        // Assert
        Assert.False(await collection.IsFinishedAndEmptyAsync(), "After getting all items collection should be empty.");
        Assert.Equal((uint)2, await collection.GetProducedCountAsync());
        Assert.Equal((uint)2, await collection.GetConsumedCountAsync());
    }
}