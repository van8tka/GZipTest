using GZipTest_1.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace GZipTest_1.Implementations
{

    internal abstract class AbstractArchiver : IArchiver, IDisposable
    {
        //ctor
        protected AbstractArchiver(string input, string output)
        {
            InputFile = input;
            OutputFile = output;
            EventWaitHandleArray = new ManualResetEvent[_countProcessors()];
            BlockReaded = new BlockingCollection<BlockData>();
            BlockForWrite = new BlockingCollection<BlockData>();
            Success = false;
            IsError = false;
        }
        //fix me: upperBoundCollection
        // private int _upperBoundCollection = 10000;
        protected bool Success;
        protected bool IsError;
        protected readonly string InputFile;
        protected readonly string OutputFile;
        protected int BlockSize = 1024 * 1024;
        protected EventWaitHandle[] EventWaitHandleArray;
        protected EventWaitHandle EventWaitHandleRead;
        protected EventWaitHandle EventWaitHandleWrite;

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
                Console.WriteLine($"\n Wait for the end of {work}..");               
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
                
                Success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debugger.Break();
                Success = false;
            }
            return Success;
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


        object _lock = new object();
        int lastAddedBlock = -1;
        protected void SetPriorityData(BlockData block)
        {
            lock (_lock)
            {
                while (lastAddedBlock != block.Number - 1)
                    Monitor.Wait(_lock, 10);
                if (lastAddedBlock == block.Number - 1)
                {
                    BlockForWrite.TryAdd(block);
                    lastAddedBlock = block.Number;
                    CountZipBlocks();
                }
            }
        }





        //todo: remove this region
        #region COUNT BLOCKS
        private int CR = 0;
        private int CW = 0;
        private int CC = 0;
        object _lock1 = new object();
        object _lock2 = new object();
        object _lock3 = new object();

        protected void CountReadBlocks()
        {
            lock (_lock1)
                CR++;
           // Console.Write($"\r                      Read blocks {CR}");
        }
        protected void CountWriteBlocks()
        {
            lock (_lock2)
                CW++;
           // Console.Write($"\r                                                        Write blocks {CW}");
        }
        protected void CountZipBlocks()
        {
            lock (_lock3)
                CC++;
            // Console.Write($"\r                                                                                        Zip blocks {CC}");
        }
        #endregion

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var i in EventWaitHandleArray)
                        i.Close();
                    EventWaitHandleRead.Close();
                    EventWaitHandleWrite.Close();
                }
                BlockReaded = null;
                BlockForWrite = null;
                disposedValue = true;
            }
        }


        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {         
            Dispose(true);           
        }
      
       
    }


}