using System;
using System.Collections.Generic;

namespace Algorithms.Utility
{
    public class ObjectPool<T> where T : PooledObject<T>, new()
    {
        private readonly Queue<T> _pool;
        private readonly Func<T> _objectGenerator;
        private readonly bool _captureLeaks;
        private readonly HashSet<T> _loanedReferences;

        public ObjectPool(Func<T> objectGenerator, int initialSize = 0, bool captureLeaks = false)
        {
            _captureLeaks = captureLeaks;
            if (captureLeaks)
                _loanedReferences = new HashSet<T>(initialSize);
            _objectGenerator = objectGenerator;
            _pool = new Queue<T>(initialSize);

            // Optionally populate the pool with initial objects
            for (int i = 0; i < initialSize; i++)
            {
                _pool.Enqueue(Create());
            }
        }

        private T Create()
        {
            T obj = _objectGenerator();
            return obj;
        }

        // Borrow an object from the pool
        public T Get()
        {
            if (!_pool.TryDequeue(out T item))
            {
                item = Create();
                Console.Error.WriteLine($"Created a new object of type: {item.GetType().FullName}, initial pool size may be too small");
            }
            if (_captureLeaks)
                _loanedReferences.Add(item);
            // No objects available in the pool, create a new one
            return item;
        }

        // Return an object to the pool
        public void Return(T item)
        {
            if (_captureLeaks)
                _loanedReferences.Remove(item);
            _pool.Enqueue(item);
        }

        ~ObjectPool()
        {
            if (_captureLeaks && _loanedReferences.Count > 0)
                Console.Error.WriteLine("Potential Memory leak detected.  All loaned objects must be returned.");
        }
    }

    public abstract class PooledObject<T> : IDisposable where T : PooledObject<T>, new()
    {
        private static ObjectPool<T> _pool;

        public static void SetInitialCapacity(int capacity, bool captureLeaks = false)
        {
            _pool = new ObjectPool<T>(() => new T(), capacity, captureLeaks);
        }

        public static void DeletePool()
        {
            _pool = null;
        }

        public static T Get()
        {
            return _pool.Get();
        }

        abstract protected void Reset();

        public void Dispose()
        {
            Reset();
            _pool.Return((T)this);
        }
    }
}
