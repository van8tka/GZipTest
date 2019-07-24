using GZipTest.Models;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest.Implementations
{
    internal class Decompression : AbstractArchiver
    {
        internal Decompression(string input, string output) : base(input, output) { }

        public override bool Start()
        {
            try
            {
                Console.WriteLine(" Started decompressing..");
                EventWaitHandleRead = new ManualResetEvent(false);
                var threadRead = new Thread(ReadData);
                threadRead.Name = "ReaderThread";
                threadRead.Start();

                var threads = new Thread[CountProcessors()];
                for (int i = 0; i < threads.Length; i++)
                {
                    EventWaitHandleArray[i] = new ManualResetEvent(false);
                    threads[i] = new Thread(new ParameterizedThreadStart(DecompressData));
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
                Console.WriteLine(e);
                return false;
            }
        }

 

        private void ReadData()
        {
            try
            {
                using (var input = new FileStream(InputFile, FileMode.Open, FileAccess.Read))
                {
                    int id = 0;
                    while (input.Position < input.Length && !IsError)
                    {
                        //читаем заголовок запакованного блока и получаем размер полезной нагрузки указанной при записи
                        var headerGzip = new byte[8];
                        input.Read(headerGzip, 0, headerGzip.Length);
                        int lenghtBlock = BitConverter.ToInt32(headerGzip, 4);

                        var bytes = new byte[lenghtBlock];
                        headerGzip.CopyTo(bytes, 0);

                        input.Read(bytes, 8, lenghtBlock - 8);

                        //  BlockReaded.AddRawBlock(new BlockData(id, bytes));
                        BlockReaded.AddBlock(new BlockData(id, bytes));
                        id++;                     
                    }
                    BlockReaded.Finish();
                    EventWaitHandleRead.Set();                   
                }
            }
            catch (Exception e)
            {                
                Console.WriteLine(e);
                IsError = true;
                EventWaitHandleRead.Set();
            }
        }



        private void DecompressData(object indexThread)
        {
            try
            {
                while (true && !IsError)
                {
                    BlockData block;
                    if (BlockReaded.TryTakeBlock(out block))
                    {
                        using (var memStream = new MemoryStream(block.Bytes))
                        {
                            using (var gzipStream = new GZipStream(memStream, CompressionMode.Decompress))
                            {
                                byte[] hederBytes = new byte[4] { block.Bytes[4], block.Bytes[5], block.Bytes[6], block.Bytes[7] };
                                int lenghtBlock = BitConverter.ToInt32(hederBytes, 0);
                                int dataLenght = BitConverter.ToInt32(block.Bytes, lenghtBlock - 4);
                                byte[] data = new byte[dataLenght];
                                gzipStream.Read(data, 0, dataLenght);
                                // BlockProcessed.AddRawBlock(new BlockData(block.Number, data));
                                BlockProcessed.AddBlock(new BlockData(block.Number, data));
                            }                           
                        }
                    }
                    else
                    {
                        EventWaitHandleArray[(int)indexThread].Set();                        
                        return;
                    }                      
                }
                EventWaitHandleArray[(int)indexThread].Set();
            }
            catch (Exception e)
            {              
                Console.WriteLine(e);
                IsError = true;
                EventWaitHandleArray[(int)indexThread].Set();
            }
        }



        private void WriteData()
        {
            try
            {
                int blocksWrite = 0;
                while (true && !IsError)
                {
                    BlockData block;
                    if (BlockProcessed.TryTakeBlock(out block))
                    {
                        using (var outputStream = new FileStream(OutputFile, FileMode.Append, FileAccess.Write))
                        {
                            outputStream.Write(block.Bytes, 0, block.Bytes.Length);
                            blocksWrite++;
                            ProgressInfo.Output(blocksWrite, BlocksCount);
                        }
                    }
                    else
                    {
                        ProgressInfo.End();
                        EventWaitHandleWrite.Set();
                        return;
                    }                       
                }
                EventWaitHandleWrite.Set();
            }
            catch (Exception e)
            {               
                Console.WriteLine(e);
                IsError = true;
                EventWaitHandleWrite.Set();
            }
        }
    }
}
