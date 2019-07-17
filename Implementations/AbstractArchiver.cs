using GZipTest_1.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace GZipTest_1.Implementations
{

    internal abstract class AbstractArchiver : IArchiver
    {
        //ctor
        protected AbstractArchiver(string input, string output)
        {
            InputFile = input;
            OutputFile = output;
            ManualResetEventArray = new ManualResetEvent[_countProcessors()];
            BlockReaded = new BlockingCollection<byte[]>(_upperBoundCollection);
            BlockForWrite = new BlockingCollection<byte[]>(_upperBoundCollection);
            Success = false;
        }

        private int _upperBoundCollection = 10000;
        protected bool Success;
        protected readonly string InputFile;
        protected readonly string OutputFile;
        protected int BlockSize = 1024 * 1024;
        protected ManualResetEvent[] ManualResetEventArray;
        protected BlockingCollection<byte[]> BlockReaded { get; set; }
        protected BlockingCollection<byte[]> BlockForWrite { get; set; }

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
            {
                Console.WriteLine($"\nWait for the end of {work}.");
                Console.WriteLine("Push any key to exit..");
                Console.ReadKey();
            }
        }

        protected bool Start(Action<object> action)
        {
            try
            {
                var threadRead = new Thread(ReadData);
                threadRead.Start();

                var threads = new Thread[_countProcessors()];
                for (int i = 0; i < threads.Length; i++)
                {
                    ManualResetEventArray[i] = new ManualResetEvent(false);
                    threads[i] = new Thread(new ParameterizedThreadStart(action));
                    threads[i].Start(i);
                }

                var threadWrite = new Thread(WriteData);
                threadWrite.Start();

                WaitHandle.WaitAll(ManualResetEventArray);
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

        public static AbstractArchiver CreateArchiver(string action, string input, string output)
        {
            if (action.Equals(Constants.COMPRESS, StringComparison.OrdinalIgnoreCase))
                return new Compression(input, output);
            else
                return new Decompression(input, output);
        }


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
            Console.Write($"\r                      Read blocks {CR}");
        }
        protected void CountWriteBlocks()
        {
            lock (_lock2)
                CW++;
            Console.Write($"\r                                                        Write blocks {CW}");
        }
        protected void CountZipBlocks()
        {
            lock (_lock3)
                CC++;
            Console.Write($"\r                                                                                        Zip blocks {CC}");
        }
    }
}