using GZipTest.Helpers;
using GZipTest.Interfaces;
using GZipTest.Models;
using System;
using System.IO;
using System.Threading;

namespace GZipTest.Implementations
{
    public abstract class AbstractArchiver : IArchiver, IDisposable
    {
        //ctor
        protected AbstractArchiver(string input, string output)
        {
            InputFile = input;
            OutputFile = output;
            EventWaitHandleArray = new ManualResetEvent[CountProcessors()];
            BlockReaded = new CustomBlockingCollection();
            BlockProcessed = new CustomBlockingCollection();
            IsError = false;           
            //todo: must remove this class
            CountBlocks = new CountBlocks(true);
        }
        //todo: must remove this class
        protected CountBlocks CountBlocks { get;private set; }
 
        private bool _disposedValue = false;
        protected bool IsError;
        protected readonly string InputFile;
        protected readonly string OutputFile;
        protected int BlocksCount { get; set; }
        protected int BlocksProcessedCount = 0;
        protected int BlockSize = 1024 * 1024;
        protected EventWaitHandle[] EventWaitHandleArray;
        protected EventWaitHandle EventWaitHandleRead;
        protected EventWaitHandle EventWaitHandleWrite;

        protected CustomBlockingCollection BlockReaded { get; set; }
        protected CustomBlockingCollection BlockProcessed { get; set; }

        protected int CountProcessors() => Environment.ProcessorCount;

        public abstract bool Start();

        protected void WaitFinish()
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

 
        protected void CheckMemory()
        {
            //var ramCounter = new PerformanceCounter("Memory", "Available MBytes");           
            //float mbFree = ramCounter.NextValue();
            //Console.Write($"\r                         free memory {mbFree} ");
        }
 
        public byte[] AddedHelpersDataToByteArray(int data, byte[] bytes)
        {
            byte[] dataBytes = BitConverter.GetBytes(data);
            var resultBytes = new byte[dataBytes.Length + bytes.Length];
            dataBytes.CopyTo(resultBytes, 0);
            bytes.CopyTo(resultBytes, dataBytes.Length);
            return resultBytes;
        }

        public byte[] GetHelpersDataFromByteArray(byte[] bytes, out int data)
        {
            int dataLenght = 4;
            byte[] bytesResult = new byte[bytes.Length - dataLenght];         
            Array.Copy(bytes, dataLenght, bytesResult,0, bytesResult.Length);           
            byte[] tempBytes = { bytes[0], bytes[1], bytes[2], bytes[3] };        
            data = BitConverter.ToInt32(tempBytes, 0);
            return bytesResult;
        }

    }
}