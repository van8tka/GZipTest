using System.Collections.Generic;
using System.Threading;

namespace GZipTest.Models

{
    public class CustomBlockingCollection
    {
        private Queue<BlockData> _queue;
        private object _lock = new object();
        private int _boundedCapacity;  

        public bool IsFinish { get; private set; }

        internal CustomBlockingCollection(int boundedCapacity)
        {
            _boundedCapacity = boundedCapacity;
            _queue = new Queue<BlockData>();
            IsFinish = false;
        }

        internal void AddBlock(BlockData block)
        {
            bool _lockWasTaken = false;
            try
            {
                Monitor.Enter(_lock, ref _lockWasTaken);
                if (!IsFinish)
                {
                    while (_queue.Count >= _boundedCapacity)
                        Monitor.Wait(_lock);
                    _queue.Enqueue(block);
                }                  
                Monitor.PulseAll(_lock);
            }
            finally
            {
                ReleaseLock(_lockWasTaken);
            }
        }

        internal bool TryTakeBlock(out BlockData block)
        {
            bool _lockWasTaken = false;
            try
            {
                Monitor.Enter(_lock, ref _lockWasTaken);
                while (_queue.Count == 0 && !IsFinish)
                    Monitor.Wait(_lock);
                if (_queue.Count != 0)
                {
                    block = _queue.Dequeue();
                    Monitor.PulseAll(_lock);
                    return true;
                }
                block = default;
                return false;
            }
            finally
            {
                ReleaseLock(_lockWasTaken);
            }
        }

        public void Finish()
        {
            bool _lockWasTaken = false;
            try
            {
                Monitor.Enter(_lock, ref _lockWasTaken);
                IsFinish = true;
                Monitor.PulseAll(_lock);
            }
            finally
            {
                ReleaseLock(_lockWasTaken);
            }
        }

        private void ReleaseLock(bool _lockWasTaken)
        {
            if (_lockWasTaken)
                Monitor.Exit(_lock);
        }
    }
}
