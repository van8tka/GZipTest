using System.Collections.Generic;
using System.Threading;

namespace GZipTest.Models

{
    internal class CustomBlockingCollection
    {
        private Queue<BlockData> _queue;
        private object _lock = new object();
        private int _number = 0;
        public bool IsFinish { get; private set; }

        internal CustomBlockingCollection()
        {
            _queue = new Queue<BlockData>();
            IsFinish = false;
        }


        internal void AddRawBlock(BlockData block)
        {
            if (!IsFinish)
            {
                Monitor.Enter(_lock);
                _queue.Enqueue(block);
                _number++;
                Monitor.PulseAll(_lock);
                Monitor.Exit(_lock);
            }
        }

        internal void AddProccessedBlock(BlockData block)
        {
            int id = block.Number;
            Monitor.Enter(_lock);
            if (!IsFinish)
            {
                while (id != _number)
                    Monitor.Wait(_lock);
                _queue.Enqueue(block);
                _number++;
                Monitor.PulseAll(_lock);
            }
            Monitor.Exit(_lock);
        }

        internal bool TryTakeBlock(out BlockData block)
        {
            try
            {
                Monitor.Enter(_lock);
                bool isTake = false;
                while (_queue.Count == 0 && !IsFinish)
                    Monitor.Wait(_lock);
                if (_queue.Count == 0)
                {
                    block = default;

                }
                else
                {
                    block = _queue.Dequeue();
                    isTake = true;
                }
                return isTake;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public void Finish()
        {
            Monitor.Enter(_lock);
            IsFinish = true;
            Monitor.PulseAll(_lock);
            Monitor.Exit(_lock);
        }

    }
}
