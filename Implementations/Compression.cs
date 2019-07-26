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
        internal Compression(string input, string output, int blockSize, int boundedCapacity) : base(input, output, blockSize, boundedCapacity)
        {
            BlocksCount = GetBlockCount(BlockSize);
        }

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
                        //добавим доп инф. о позиции массива байт в файле и размере файла
                        bytes = AddedHelpersData(input.Position - readCount, lenght, bytes);
                        BlockReaded.AddBlock(new BlockData(bytes));
                    }
                    BlockReaded.Finish();                  
                }
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine(Environment.NewLine + "Not enough RAM to complete file compression. Please close other applications and try again.");
                IsError = true;               
            }
            catch (Exception e)
            {
                Console.WriteLine(Environment.NewLine + e);
                IsError = true;                
            }
            finally
            {
                EventWaitHandleRead.Set();
            }
        }
        /// <summary>
        /// добавление в массив байт 8 байт несущих доп. инф.: первые 4 байта - позиция массива байт, вторые 4 байта - размер файла до архивации
        /// </summary>       
        /// <returns>массив байт с добавленной доп. информацией 8 байт</returns>
        private byte[] AddedHelpersData(long position, long lenght, byte[] bytes)
        {
            return DataManager.AddedHelpersDataToByteArray(position, DataManager.AddedHelpersDataToByteArray(lenght, bytes));
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
                            BlockProcessed.AddBlock(new BlockData(memStream.ToArray()));
                            BlocksProcessedCount++;
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
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine(Environment.NewLine + "Not enough RAM to complete file compression. Please close other applications and try again.");
                IsError = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(Environment.NewLine + e);
                IsError = true;
            }
            finally
            {
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
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine(Environment.NewLine + "Not enough RAM to complete file compression. Please close other applications and try again.");
                IsError = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(Environment.NewLine + e);
                IsError = true;
              
            }
            finally
            {
                EventWaitHandleWrite.Set();
            }
        }

        /// <summary>
        /// получение общего колличества блоков в зависимости от исходного размера входящего файла
        /// </summary>       
        private int GetBlockCount(int blockSize)
        {
            if (string.IsNullOrEmpty(InputFile))
                return 0;
            var file = new FileInfo(InputFile);
            return (int)Math.Ceiling((double)file.Length / blockSize);
        }
    }
}