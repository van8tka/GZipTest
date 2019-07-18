using GZipTest.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace GZipTest.Implementations
{
    internal abstract class AbstractArchiver : IArchiver, IDisposable
    {
        //ctor
        protected AbstractArchiver(string input, string output)
        {
            InputFile = input;
            OutputFile = output;
            EventWaitHandleArray = new ManualResetEvent[_countProcessors()];
            SetAvailableBlockBounds();
            BlockReaded = new BlockingCollection<BlockData>(BlockBound);
            BlockForWrite = new BlockingCollection<BlockData>(BlockBound);
           
            IsError = false;
        }

        private void SetAvailableBlockBounds()
        {
            var ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            int mbSize = 1024 * 1024;
            BlockBound = (int)(ramCounter.NextValue() * mbSize / BlockSize) / 4;
        }

        private bool _disposedValue = false;
        private object _lock = new object();
        private int _lastAddedBlock = -1;
      
        protected bool IsError;
        protected readonly string InputFile;
        protected readonly string OutputFile;
        protected int BlockSize = 1024 * 1024;
        protected EventWaitHandle[] EventWaitHandleArray;
        protected EventWaitHandle EventWaitHandleRead;
        protected EventWaitHandle EventWaitHandleWrite;

        protected int BlockBound;

        protected BlockingCollection<BlockData> BlockReaded { get; set; }
        protected BlockingCollection<BlockData> BlockForWrite { get; set; }

        private int _countProcessors() => Environment.ProcessorCount;
        public abstract bool Start();
        public abstract void ReadData();
        public abstract void WriteData();

        protected void OutputProgress(long position, long length, string work)
        {
            int persent = 100;
            int currentPersent = (int)(position * persent / length);
            Console.Write($"\r progress: {currentPersent} %");
            if (currentPersent == persent)
                Console.WriteLine($"\n Please, wait for the end of {work}..");               
        }

        protected bool Start(Action<object> action)
        {
            try
            {
                EventWaitHandleRead = new ManualResetEvent(false);
                var threadRead = new Thread(ReadData);
                threadRead.Start();
 
                var threads = new Thread[_countProcessors()];
                for (int i = 0; i < threads.Length; i++)
                {
                    EventWaitHandleArray[i] = new ManualResetEvent(false);
                    threads[i] = new Thread(new ParameterizedThreadStart(action));
                    threads[i].Start(i);
                }
                EventWaitHandleWrite = new ManualResetEvent(false);
                var threadWrite = new Thread(WriteData);
                threadWrite.Start();
                WaitFinish();
                return !IsError;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);               
                return false;               
            }            
        }

        private void WaitFinish()
        {
            var handle = new WaitHandle[EventWaitHandleArray.Length + 2];
            EventWaitHandleArray.CopyTo(handle, 2);
            handle[0] = EventWaitHandleRead;
            handle[1] = EventWaitHandleWrite;
            WaitHandle.WaitAll(handle);
        }

        public static AbstractArchiver CreateArchiver(string action, string input, string output)
        {
            if (action.Equals(Constants.COMPRESS, StringComparison.OrdinalIgnoreCase))
                return new Compression(input, output);
            else
                return new Decompression(input, output);
        }

       
        protected void SetPriorityData(BlockData block)
        {
            try
            {
                lock (_lock)
                {
                    while (_lastAddedBlock != block.Number - 1)
                        Monitor.Wait(_lock, 10);
                    if (_lastAddedBlock == block.Number - 1)
                    {
                        while (BlockForWrite.Count == BlockForWrite.BoundedCapacity)
                            Monitor.Wait(_lock, 10);
                        BlockForWrite.Add(block);
                            _lastAddedBlock = block.Number;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw;
            }           
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var handle in EventWaitHandleArray)
                        handle.Close();
                    EventWaitHandleRead.Close();
                    EventWaitHandleWrite.Close();
                    BlockReaded.Dispose();
                    BlockForWrite.Dispose();
                }              
                _disposedValue = true;
            }
        }
      
        public void Dispose()
        {         
            Dispose(true);           
        }
    }


}