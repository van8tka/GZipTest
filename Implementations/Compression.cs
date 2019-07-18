using GZipTest_1.Interfaces;
using System;
using System.IO;
using System.IO.Compression;
 

namespace GZipTest_1.Implementations
{
    internal class Compression : AbstractArchiver, ICompression
    {
        //ctor
        internal Compression(string inputfile, string outputfile) : base(inputfile, outputfile) { }


        public override bool Start()
        {
            Console.WriteLine(" Started compressing..");
            return Start(CompressData);
        }

        public override void ReadData()
        {
            try
            {
                int id = 0;
                using (var input = new FileStream(InputFile, FileMode.Open, FileAccess.Read))
                {
                    var lenght = input.Length;
                    while (input.Position < lenght && !IsError)
                    {
                        int readCount;
                        if (lenght - input.Position < BlockSize)
                            readCount = (int)(lenght - input.Position);
                        else
                            readCount = BlockSize;
                        var bytes = new byte[readCount];
                        input.Read(bytes, 0, readCount);                     
                        BlockReaded.TryAdd(new BlockData(id, bytes));
                        id++;
                        OutputProgress(input.Position, input.Length, "compression");
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


        public void CompressData(object indexThread)
        {
            try
            {
                while (true && !IsError)
                {
                    BlockData block;
                    if (BlockReaded.TryTake(out block, 1000))
                    {
                        using (var memStream = new MemoryStream())
                        {
                            using (var gzipStream = new GZipStream(memStream, CompressionMode.Compress))
                            {
                                gzipStream.Write(block.Bytes, 0, block.Bytes.Length);
                            }                         
                            SetPriorityData(new BlockData(block.Number, memStream.ToArray()));
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
                while (true && ! IsError)
                {
                    using (var outputStream = new FileStream(OutputFile, FileMode.Append, FileAccess.Write))
                    {

                        BlockData block;
                        if (BlockForWrite.TryTake(out block, 1000))
                        {                           
                            //получим размер сжатых данных для последующей декомпрессии
                            var lenghtOfBlock = BitConverter.GetBytes(block.Bytes.Length);
                            /*запишем информацию о размере блока для последующей декомперссии и записи, вместо
                             времени модификации файла в формате  MTIME  (согласно спецификации gzip) */
                            lenghtOfBlock.CopyTo(block.Bytes, 4);
                            outputStream.Write(block.Bytes, 0, block.Bytes.Length);                        
                        }
                        else
                        {
                            EventWaitHandleWrite.Set();
                            return;
                        }                            
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