using GZipTest.Models;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest.Implementations
{
    internal class Compression : AbstractArchiver
    {
        //ctor
        internal Compression(string inputfile, string outputfile) : base(inputfile, outputfile) { }

        public override bool Start()
        {
            try
            {
                Console.WriteLine(" Started compressing..");
                EventWaitHandleRead = new ManualResetEvent(false);
                var threadRead = new Thread(ReadData);
                threadRead.Start();

                var threads = new Thread[CountProcessors()];
                for (int i = 0; i < threads.Length; i++)
                {
                    EventWaitHandleArray[i] = new ManualResetEvent(false);
                    threads[i] = new Thread(new ParameterizedThreadStart(CompressData));
                    threads[i].Start(i);
                }
                EventWaitHandleWrite = new ManualResetEvent(false);
                var threadWrite = new Thread(WriteData);
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
                   
                        BlockReaded.AddRawBlock(new BlockData(id, bytes));                       
                        id++;                     
                        //CountBR();
                    }
                    //BlockReaded.Finish();
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


        private void CompressData(object indexThread)
        {
            try
            {
                while (true && !IsError)
                {
                    BlockData block;
                    if (BlockReaded.TryTakeBlock(out block))
                    {
                        using (var memStream = new MemoryStream())
                        {
                            using (var gzipStream = new GZipStream(memStream, CompressionMode.Compress))
                            {
                                gzipStream.Write(block.Bytes, 0, block.Bytes.Length);
                            }
                            BlockProcessed.AddProccessedBlock(new BlockData(block.Number, memStream.ToArray()));
                            //CountBZ();
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
                EventWaitHandleArray[(int)indexThread].Set();
                Console.WriteLine(e);
                IsError = true; 
            }
        }


        private void WriteData()
        {
            try
            {
                int blocksWrite = 0;
                while (true && ! IsError)
                {
                    using (var outputStream = new FileStream(OutputFile, FileMode.Append, FileAccess.Write))
                    {
                        BlockData block;
                        if (BlockProcessed.TryTakeBlock(out block))
                        {                           
                            //получим размер сжатых данных для последующей декомпрессии
                            var lenghtOfBlock = BitConverter.GetBytes(block.Bytes.Length);
                            /*запишем информацию о размере блока для последующей декомперссии и записи, вместо
                             времени модификации файла в формате  MTIME  (согласно спецификации gzip) */
                            lenghtOfBlock.CopyTo(block.Bytes, 4);
                            outputStream.Write(block.Bytes, 0, block.Bytes.Length);
                            blocksWrite++;
                            ProgressInfo.Output(blocksWrite, BlocksCount);
                            //CountBW();
                        }
                        else
                        {
                            ProgressInfo.End();
                            EventWaitHandleWrite.Set();
                            return;
                        }                            
                    }
                }
                EventWaitHandleWrite.Set();
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