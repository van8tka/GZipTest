using GZipTest.Models;
using System;
using System.Diagnostics;
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
                threadRead.Name = "ReaderThread";
                threadRead.Start();

                var threads = new Thread[CountProcessors()];
                for (int i = 0; i < threads.Length; i++)
                {
                    EventWaitHandleArray[i] = new ManualResetEvent(false);
                    threads[i] = new Thread(new ParameterizedThreadStart(CompressData));
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
            catch (OutOfMemoryException e)
            {
                Debugger.Break();
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(Environment.NewLine + e);
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
                        CheckMemory();
                        int readCount;
                        if (lenght - input.Position < BlockSize)
                            readCount = (int)(lenght - input.Position);
                        else
                            readCount = BlockSize;
                        var bytes = new byte[readCount];
                        input.Read(bytes, 0, readCount);                                           
                        BlockReaded.AddBlock(new BlockData(id, bytes));
                        id++;
                        CountBlocks.CountBR();
                    }
                    BlockReaded.Finish();
                    EventWaitHandleRead.Set();
                }
            }
            catch(OutOfMemoryException e)
            {
                Debugger.Break();
            }
            catch (Exception e)
            {
                Console.WriteLine(Environment.NewLine + e);
                IsError = true;
                EventWaitHandleRead.Set();
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
                                CheckMemory();
                                gzipStream.Write(block.Bytes, 0, block.Bytes.Length);
                            }                            
                            BlockProcessed.AddBlock(new BlockData(block.Number, memStream.ToArray()));
                            BlocksProcessedCount++;
                            CountBlocks.CountBZ();
                        }                       
                    }
                    else
                    {
                        if (BlockReaded.IsFinish && BlocksProcessedCount == BlocksCount)
                            BlockProcessed.Finish();
                        EventWaitHandleArray[(int)indexThread].Set();
                        return;
                    }                       
                }                
                EventWaitHandleArray[(int)indexThread].Set();
            }
            catch (OutOfMemoryException e)
            {
                Debugger.Break();
            }
            catch (Exception e)
            {
                Console.WriteLine(Environment.NewLine + e);
                IsError = true;
                EventWaitHandleArray[(int)indexThread].Set();
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
                        CheckMemory();
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
                            CountBlocks.CountBW();
                        }
                        else
                        {
                            if (blocksWrite == BlocksCount)
                                ProgressInfo.End();
                            else
                                throw new Exception("Can't write all blocks");
                            EventWaitHandleWrite.Set();
                            return;
                        }                            
                    }
                }
                EventWaitHandleWrite.Set();
            }
            catch (OutOfMemoryException e)
            {
                Debugger.Break();
            }
            catch (Exception e)
            {              
                Console.WriteLine(Environment.NewLine+e);
                IsError = true;
                EventWaitHandleWrite.Set();
            }
        }
    }
}