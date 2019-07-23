using System.Collections.Generic;
using System.Threading;

namespace GZipTest.Implementations
{
    internal class CustomBlockingCollection
    {
        private Queue<BlockData> _queue;
        private object _lock = new object();
        private int _number = 0;
        private bool _isFinish = false;
    
        internal CustomBlockingCollection()
        {
            _queue = new Queue<BlockData>();
        }


        internal void AddRawBlock(BlockData block)
        {
            if (!_isFinish)
            {
                lock (_lock)
                {
                    _queue.Enqueue(block);
                    _number++;
                    Monitor.PulseAll(_lock);
                }
            }
        }

        internal void AddProccessedBlock(BlockData block)
        {
            if (!_isFinish)
            {
                int id = block.Number;
                lock (_lock)
                {
                    while (id != _number)
                    {
                        Monitor.Wait(_lock);
                    }
                    _queue.Enqueue(block);
                    _number++;
                    Monitor.PulseAll(_lock);
                }
            }
        }

        internal BlockData TakeBlock()
        {
            lock(_lock)
            {
                while (_queue.Count == 0 && !_isFinish)
                    Monitor.Wait(_lock);
                return _queue.Dequeue();
            }
        }



        public void Finish()
        {
            lock (_lock)
            {
                _isFinish = true;
                Monitor.PulseAll(_lock);
            }
        }

    }
}
