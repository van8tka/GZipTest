using System.Collections.Generic;
using System.Threading;

namespace GZipTest.Models

{
    internal class CustomBlockingCollection
    {
        private Queue<BlockData> _queue;
        private object _lock = new object();
     //   byte _countCurrentThreads = 0;

        //      private int _number = 0;
        public bool IsFinish { get; private set; }

        internal CustomBlockingCollection()
        {
            _queue = new Queue<BlockData>();
            IsFinish = false;
        }

       

        internal void AddBlock(BlockData block)
        {
            bool _lockWasTaken = false;
            try
            {
              //  _countCurrentThreads++;
                Monitor.Enter(_lock, ref _lockWasTaken);               
                if (!IsFinish)
                    _queue.Enqueue(block);             
                Monitor.PulseAll(_lock);
            }
            finally
            {
             //   _countCurrentThreads--;
                 ReleaseLock(_lockWasTaken);
            }
        }


        //internal void AddRawBlock(BlockData block)
        //{
        //    bool _lockWasTaken = false;
        //    try
        //    {              
        //        Monitor.Enter(_lock, ref _lockWasTaken);
        //        if (!IsFinish)
        //            _queue.Enqueue(block);
        //             //     _number++;
        //        Monitor.PulseAll(_lock);
        //    }
        //    finally
        //    {
        //        ReleaseLock(_lockWasTaken);
        //    }
        //}

        //internal void AddProccessedBlock(BlockData block)
        //{
        //    bool _lockWasTaken = false;
        //    try
        //    {
        //          int id = block.Number;
        //        Monitor.Enter(_lock, ref _lockWasTaken);
        //        if (!IsFinish)
        //        {
        //             //while (id != _number)
        //             //    Monitor.Wait(_lock);
        //            _queue.Enqueue(block);
        //               //  _number++;
        //            Monitor.PulseAll(_lock);
        //        }
        //    }
        //    finally
        //    {
        //        ReleaseLock(_lockWasTaken);
        //    }
        //}

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
                //while (_countCurrentThreads != 0)
                //    Monitor.Wait(_lock);              
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
