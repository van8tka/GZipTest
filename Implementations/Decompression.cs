using GZipTest.Interfaces;
using System;
using System.IO;
using System.IO.Compression;


namespace GZipTest.Implementations
{
    internal class Decompression : AbstractArchiver, IDecompression
    {
        internal Decompression(string input, string output) : base(input, output) { }

        public override bool Start()
        {
            Console.WriteLine(" Started decompressing..");
            return Start(DecompressData);
        }


        public override void ReadData()
        {
            try
            {
                using (var input = new FileStream(InputFile, FileMode.Open, FileAccess.Read))
                {
                    int id = 0;
                    while (input.Position < input.Length && !IsError)
                    {
                        //читаем заголовок запакаованного блока и получаем размер полезной нагрузки указанной при записи
                        var headerGzip = new byte[8];
                        input.Read(headerGzip, 0, headerGzip.Length);
                        int lenghtBlock = BitConverter.ToInt32(headerGzip, 4);

                        var bytes = new byte[lenghtBlock];
                        headerGzip.CopyTo(bytes, 0);

                        input.Read(bytes, 8, lenghtBlock - 8);                      
                        BlockReaded.TryAdd(new BlockData(id, bytes));
                        id++;
                        OutputProgress(input.Position, input.Length, "decompression");
                    }
                    EventWaitHandleRead.Set();
                }
            }
            catch (Exception e)
            {
                EventWaitHandleRead.Set();
                Console.WriteLine(e);
                IsError = true;
            }
        }



        public void DecompressData(object indexThread)
        {
            try
            {

                while (true && !IsError)
                {
                    BlockData block;
                    if (BlockReaded.TryTake(out block, 1000))
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
                                SetPriorityData(new BlockData(block.Number, data));
                            }
                           
                        }
                    }
                    else
                    {
                        EventWaitHandleArray[(int)indexThread].Set();
                        return;
                    }                      
                }
            }
            catch (Exception e)
            {
                EventWaitHandleArray[(int)indexThread].Set();
                Console.WriteLine(e);
                IsError = true;
            }
        }



        public override void WriteData()
        {
            try
            {
                while (true && !IsError)
                {
                    BlockData block;
                    if (BlockForWrite.TryTake(out block, 1000))
                    {
                        using (var outputStream = new FileStream(OutputFile, FileMode.Append, FileAccess.Write))
                        {
                            outputStream.Write(block.Bytes, 0, block.Bytes.Length);                           
                        }
                    }
                    else
                    {
                        EventWaitHandleWrite.Set();
                        return;
                    }                       
                }
            }
            catch (Exception e)
            {
                EventWaitHandleWrite.Set();
                Console.WriteLine(e);
                IsError = true; 
            }
        }
    }
}
