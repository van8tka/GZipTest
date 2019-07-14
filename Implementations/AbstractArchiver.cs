using GZipTest_1.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace GZipTest_1.Implementations
{

    internal abstract class  AbstractArchiver:IArchiver
    {
        //ctor
        protected AbstractArchiver( string input, string output )
        {
            InputFile = input;
            OutputFile = output;
            ManualResetEventArray = new ManualResetEvent[CountProcessors()];
            BlockReaded = new BlockingCollection<byte[]>(_upperBoundCollection);
            BlockForWrite = new BlockingCollection<byte[]>(_upperBoundCollection);
            Success = false;
        }

        private int _upperBoundCollection = 1000;
        protected bool Success;
        protected readonly string InputFile;
        protected readonly string OutputFile;
        protected int BlockSize = 1024 * 1024;
        protected ManualResetEvent[] ManualResetEventArray;
        protected BlockingCollection<byte[]> BlockReaded { get; set; }
        protected BlockingCollection<byte[]> BlockForWrite { get; set; }
        
        protected int CountProcessors() => Environment.ProcessorCount;
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
    }
}