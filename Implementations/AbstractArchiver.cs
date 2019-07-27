using GZipTest.Helpers;
using GZipTest.Interfaces;
using GZipTest.Models;
using System;
using System.Threading;

namespace GZipTest.Implementations
{
    public abstract class AbstractArchiver : IArchiver, IDisposable
    {
        //ctor
        protected AbstractArchiver(string input, string output, int blockSize, int boundedCapacity)
        {
            InputFile = input;
            OutputFile = output;
            CountProcessors = GetCountProcessors();
            EventWaitHandleArray = new ManualResetEvent[CountProcessors];
            BlockReaded = new CustomBlockingCollection(boundedCapacity);
            BlockProcessed = new CustomBlockingCollection(boundedCapacity);
            DataManager = new AdditionalDataManager();
            IsError = false;
            BlockSize = blockSize;          
        }
             
        protected readonly AdditionalDataManager DataManager;
        protected readonly string InputFile;
        protected readonly string OutputFile;
        protected readonly int BlockSize;
        protected int BlocksProcessedCount = 0;
        private bool _disposedValue = false;
        protected EventWaitHandle[] EventWaitHandleArray;
        protected EventWaitHandle EventWaitHandleRead;
        protected EventWaitHandle EventWaitHandleWrite;
       

        protected int BlocksCount { get; set; }
        protected bool IsError { get; set; }
        protected CustomBlockingCollection BlockReaded { get; set; }
        protected CustomBlockingCollection BlockProcessed { get; set; }
        protected int CountProcessors {get;set;}
        private int GetCountProcessors() => Environment.ProcessorCount;
        public abstract bool Start();

        protected void WaitFinish()
        {
            var handle = new WaitHandle[EventWaitHandleArray.Length + 2];
            EventWaitHandleArray.CopyTo(handle, 2);
            handle[0] = EventWaitHandleRead;
            handle[1] = EventWaitHandleWrite;
            foreach (var e in handle)
                e.WaitOne();
          //  WaitHandle.WaitAll(handle);
        }

        public static AbstractArchiver CreateArchiver(string action, string input, string output, int blockSize, int boundedCapacity)
        {
            if (action.Equals(Constants.COMPRESS, StringComparison.OrdinalIgnoreCase))
                return new Compression(input, output, blockSize, boundedCapacity);
            else
                return new Decompression(input, output, blockSize, boundedCapacity);
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