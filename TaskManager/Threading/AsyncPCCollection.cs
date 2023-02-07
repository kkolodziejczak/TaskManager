using TaskManager.Common;

namespace TaskManager.Threading
{
    public class AsyncPCCollection<T> : IAsyncProducer<T>, IAsyncConsumer<T>, IDisposable
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

        public async Task<T?> GetAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                if (_items.IsNotEmpty() && !_finished)
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
                if (_items.IsNotEmpty() && !_finished)
                {
                    var items = new T[_items.Count];
                    for (var i = 0; i < _items.Count; i++)
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

        public async Task<bool> IsFinishedAndEmptyAsync(CancellationToken token)
        {
            try
            {
                await _semaphore.WaitAsync(token);
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

        public void Dispose()
        {
            FinishAsync().GetAwaiter().GetResult();
            _semaphore.Dispose();
        }
    }
}