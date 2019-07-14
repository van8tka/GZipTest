using GZipTest_1.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest_1.Implementations
{
    internal class Decompression : AbstractArchiver, IDecompression
    {
        internal Decompression(string input, string output) : base(input, output) { }

        public override bool Start()
        {
            try
            {
                Console.WriteLine("Started decompressing..");
                var threadRead = new Thread(ReadData);
                threadRead.Start();

                var threads = new Thread[CountProcessors()];
                for (int i = 0; i < threads.Length; i++)
                {
                    ManualResetEventArray[i] = new ManualResetEvent(false);
                    threads[i] = new Thread(DecompressData);
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


        public override void ReadData()
        {
            try
            {
                using (var input = new FileStream(InputFile, FileMode.Open, FileAccess.Read))
                {
                   
                    while (input.Position < input.Length)
                    {
                        var headerGzip = new byte[8];
                        input.Read(headerGzip, 0, headerGzip.Length);
                        int lenghtBlock = BitConverter.ToInt32(headerGzip, 4);

                        var bytes = new byte[lenghtBlock];
                        headerGzip.CopyTo(bytes, 0);

                        input.Read(bytes, 8, lenghtBlock - 8);
                        BlockReaded.TryAdd(bytes);
                        OutputProgress(input.Position, input.Length, "decompression");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debugger.Break();
            }
        }

        public void DecompressData(object indexThread)
        {
            try
            {

                while (true)
                {
                    byte[] bytes;
                    if (BlockReaded.TryTake(out bytes, 1000))
                    {
                        using (var memStream = new MemoryStream(bytes))
                        {
                            using (var gzipStream = new GZipStream(memStream, CompressionMode.Decompress))
                            {
                                byte[] hederBytes = new byte[4] {bytes[4], bytes[5], bytes[6], bytes[7] };                              
                                int lenghtBlock = BitConverter.ToInt32(hederBytes, 0);
                                int dataLenght = BitConverter.ToInt32(bytes, lenghtBlock - 4);
                                byte[] data = new byte[dataLenght];
                                gzipStream.Read(data, 0, dataLenght);
                                BlockForWrite.TryAdd(data);
                            }
                            ManualResetEventArray[(int)indexThread].Set();
                        }
                    }
                    else
                       return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debugger.Break();
            }
        }



        public override void WriteData()
        {
            try
            {
                while (true)
                {
                    byte[] bytes;
                    if (BlockForWrite.TryTake(out bytes, 1000))
                    {
                        using (var outputStream = new FileStream(OutputFile, FileMode.Append, FileAccess.Write))
                        {
                            outputStream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debugger.Break();
            }
        }
    }
}
