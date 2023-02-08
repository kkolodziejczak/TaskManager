using TaskManager.Common;

namespace TaskManager.Threading
{
    public class AsyncPCCollection<T> : IAsyncProducer<T>, IAsyncConsumer<T>
        where T : class
    {
        private readonly Queue<T> _items;
        private readonly SemaphoreSlim _semaphore;
        private uint _totalConsumed;
        private uint _totalProduced;
        private bool _finished;

        public AsyncPCCollection()
            : this(Array.Empty<T>())
        {
        }

        public AsyncPCCollection(IEnumerable<T> items)
        {
            _items = new Queue<T>(items);
            _semaphore = new SemaphoreSlim(1, 1);
            if (_items.IsNotEmpty())
            {
                FinishAsync().GetAwaiter().GetResult();
            }
        }

        ~AsyncPCCollection()
        {
            _semaphore.Dispose();
        }
        
        public async Task<T?> GetAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                if (_items.IsNotEmpty())
                {
                    var item = _items.Dequeue();
                    _totalConsumed++;
                    return item;
                }

                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IEnumerable<T>?> GetAllAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                if (_items.IsNotEmpty())
                {
                    var itemsToReturn = _items.Count;
                    var items = new T[itemsToReturn];
                    for (var i = 0; i < itemsToReturn; i++)
                    {
                        items[i] = _items.Dequeue();
                        _totalConsumed++;
                    }

                    return items;
                }

                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> IsEmptyAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                return _items.IsEmpty();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> IsFinishedAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                return _finished;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<uint> GetConsumedCountAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                return _totalConsumed;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ProduceAsync(T item, CancellationToken token)
        {
            try
            {
                await _semaphore.WaitAsync(token);
                _items.Enqueue(item);
                _totalProduced++;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ProduceAsync(IEnumerable<T> items, CancellationToken token)
        {
            try
            {
                await _semaphore.WaitAsync(token);
                foreach (var item in items)
                {
                    _items.Enqueue(item);
                    _totalProduced++;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task FinishAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                if (!_finished)
                {
                    _finished = true;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> IsFinishedAndEmptyAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                return _finished && _items.IsEmpty();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<uint> GetProducedCountAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                return _totalProduced;
            }
            finally
            {
                _semaphore.Release();
            }
        }

    }
}