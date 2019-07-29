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
        protected AbstractArchiver(string input, string output, int blockSize, int boundedCapacity, int countProccessingThread)
        {
            InputFile = input;
            OutputFile = output;
            CountProccessingThreads = countProccessingThread;
            EventWaitHandleArray = new ManualResetEvent[CountProccessingThreads];
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
        protected int CountProccessingThreads {get;set;}
       
        protected abstract void ProccessingData(object indexThread);
        protected abstract void ReadData();
        protected abstract void WriteData();

        public bool Start()
        {
            try
            {            
                EventWaitHandleRead = new ManualResetEvent(false);
                var threadRead = new Thread(ReadData);
                threadRead.Name = "ReaderThread";
                threadRead.Start();

                var threads = new Thread[CountProccessingThreads];
                for (int i = 0; i < threads.Length; i++)
                {
                    EventWaitHandleArray[i] = new ManualResetEvent(false);
                    threads[i] = new Thread(new ParameterizedThreadStart(ProccessingData));
                    threads[i].Name = $"ZipThred_{i}";
                    threads[i].Start(i);
                }
                EventWaitHandleWrite = new ManualResetEvent(false);
                var threadWrite = new Thread(WriteData);
                threadWrite.Name = "WriterThread";
                threadWrite.Start();
                WaitFinish();
                return !IsError;
            }
            catch (Exception e)
            {
                Console.WriteLine(Environment.NewLine + e);
                return false;
            }
        }
  
        private void WaitFinish()
        {
            var handle = new WaitHandle[EventWaitHandleArray.Length + 2];
            EventWaitHandleArray.CopyTo(handle, 2);
            handle[0] = EventWaitHandleRead;
            handle[1] = EventWaitHandleWrite;
            foreach (var e in handle)
                e.WaitOne();
        }

        protected void ErrorOutput( Exception e, string message = default)
        {
            Console.WriteLine(Environment.NewLine + message + e);
            IsError = true;
        }

        public static AbstractArchiver CreateArchiver(string action, string input, string output, int blockSize, int boundedCapacity, int countProccessingThread)
        {
            if (action.Equals(Constants.COMPRESS, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(" Started compressing..");
                return new Compression(input, output, blockSize, boundedCapacity, countProccessingThread);
            }              
            else
            {
                Console.WriteLine(" Started decompressing..");
                return new Decompression(input, output, blockSize, boundedCapacity, countProccessingThread);
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