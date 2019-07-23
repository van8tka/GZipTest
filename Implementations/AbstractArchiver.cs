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
            EventWaitHandleArray = new ManualResetEvent[CountProcessors()];
          //  SetAvailableBlockBounds();
            BlockReaded = new CustomBlockingCollection( );
            BlockProcessed = new CustomBlockingCollection( );
           
            IsError = false;
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

      //  protected int BlockBound;

        protected CustomBlockingCollection BlockReaded { get; set; }
        protected CustomBlockingCollection BlockProcessed { get; set; }

        protected int CountProcessors() => Environment.ProcessorCount;

        public abstract bool Start();


        //private void SetAvailableBlockBounds()
        //{
        //    var ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        //    int mbSize = 1024 * 1024;
        //    BlockBound = (int)(ramCounter.NextValue() * mbSize / BlockSize) / 4;
        //}

        protected void OutputProgress(long position, long length, string work)
        {
            int persent = 100;
            int currentPersent = (int)(position * persent / length);
            Console.Write($"\r progress: {currentPersent} %");
            if (currentPersent == persent)
                Console.WriteLine($"\n Please, wait for the end of {work}..");               
        }

           

        public static AbstractArchiver CreateArchiver(string action, string input, string output)
        {
            if (action.Equals(Constants.COMPRESS, StringComparison.OrdinalIgnoreCase))
                return new Compression(input, output);
            else
                return new Decompression(input, output);
        }

       
        //protected void SetPriorityData(BlockData block)
        //{
        //    try
        //    {
        //        lock (_lock)
        //        {
        //            while (_lastAddedBlock != block.Number - 1)
        //                Monitor.Wait(_lock, 10);
        //            if (_lastAddedBlock == block.Number - 1)
        //            {
        //                while (BlockProcessed.Count == BlockProcessed.BoundedCapacity)
        //                    Monitor.Wait(_lock, 10);
        //                BlockProcessed.Add(block);
        //                    _lastAddedBlock = block.Number;
        //            }
        //        }
        //    }
        //    catch(Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }           
        //}

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
                    //BlockReaded.Dispose();
                    //BlockProcessed.Dispose();
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