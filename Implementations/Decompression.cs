using GZipTest.Models;
using System;
using System.IO;
using System.IO.Compression;

namespace GZipTest.Implementations
{
    internal class Decompression : AbstractArchiver
    {
        internal Decompression(string input, string output, int blockSize, int boundedCapacity) : base(input, output, blockSize, boundedCapacity) { }

        protected override void ReadData()
        {
            try
            {
                using (var input = new FileStream(InputFile, FileMode.Open, FileAccess.Read))
                {                  
                    while (input.Position < input.Length && !IsError)
                    {
                        //читаем заголовок запакованного блока и получаем размер полезной нагрузки указанной при записи
                        var headerGzip = new byte[8];
                        input.Read(headerGzip, 0, headerGzip.Length);
                        int lenghtBlock = BitConverter.ToInt32(headerGzip, 4);
                        var bytes = new byte[lenghtBlock];
                        headerGzip.CopyTo(bytes, 0);
                        input.Read(bytes, 8, lenghtBlock - 8);
                        BlockReaded.AddBlock(new BlockData(bytes));                     
                    }
                    BlockReaded.Finish();
                    EventWaitHandleRead.Set();
                }
            }
            catch (OutOfMemoryException e)
            {
                ErrorOutput(e, "Not enough RAM to complete file read. Please close other applications and try again. ");
            }
            catch (Exception e)
            {
                ErrorOutput(e);
            }
            finally
            {
                EventWaitHandleRead.Set();
            }
        }

        protected override void ProccessingData(object indexThread)
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
                                //отделяем от data первые 8 байт - 4 байта позиция записи, 4 байта размер файла
                                long position;
                                long sizefile;
                                data = GetHelpersData(out position, out sizefile, data);
                                BlockProcessed.AddBlock(new BlockData(position, sizefile, data));
                                BlocksProcessedCount++;
                            }
                        }
                    }
                    else
                    {
                        if (BlockReaded.IsFinish && BlocksProcessedCount == BlocksCount)
                            BlockProcessed.Finish();
                        return;
                    }
                }              
            }
            catch (IOException e)
            {
                ErrorOutput(e, "Unable to complete decompression operation due to insufficient RAM. Please close other applications and try again. ");
            }
            catch (OutOfMemoryException e)
            {
                ErrorOutput(e, "Not enough RAM to complete file decompression. Please close other applications and try again. ");
            }
            catch (Exception e)
            {
                ErrorOutput(e);
            }
            finally
            {
                EventWaitHandleArray[(int)indexThread].Set();
            }
        }       

        protected override void WriteData()
        {
            try
            {
                bool isSetLenght = false;
                int blocksWrite = 0;
                while (true && !IsError)
                {
                    BlockData block;
                    if (BlockProcessed.TryTakeBlock(out block))
                    {
                        using (var outputStream = new FileStream(OutputFile, FileMode.Open, FileAccess.Write))
                        {
                            if (!isSetLenght)
                            {
                                outputStream.SetLength(block.SizeFile);
                                isSetLenght = true;
                            }
                            outputStream.Seek(block.Position, SeekOrigin.Begin);
                            outputStream.Write(block.Bytes, 0, block.Bytes.Length);
                            blocksWrite++;
                            ProgressInfo.Output(blocksWrite, BlocksCount);
                        }
                    }
                    else
                    {
                        if (blocksWrite >= BlocksCount)
                            ProgressInfo.End();
                        else
                            throw new Exception("Can't write all blocks");
                        return;
                    }
                }              
            }
            catch (IOException e)
            {
                ErrorOutput(e, "Unable to complete write operation due to insufficient RAM. Please close other applications and try again. ");
            }
            catch (OutOfMemoryException e)
            {
                ErrorOutput(e, "Not enough RAM to complete file write. Please close other applications and try again. ");
            }
            catch (Exception e)
            {
                ErrorOutput(e);
            }
            finally
            {
                EventWaitHandleWrite.Set();
            }
        }

        /// <summary>
        /// метод для получения позиции массива байт, размера файла до архивации, и массив байт несущих полезную нагрузку(данных файла) 
        /// </summary>       
        private byte[] GetHelpersData(out long position, out long sizefile, byte[] data)
        {
            var tempBytes = DataManager.GetHelpersDataFromByteArray(data, out position);
            tempBytes = DataManager.GetHelpersDataFromByteArray(tempBytes, out sizefile);
            if (BlocksCount == 0)
                BlocksCount = GetBlockCount(BlockSize, sizefile);
            return tempBytes;
        }
        /// <summary>
        /// метод получения общего количества блоков исходя из размера файла 
        /// </summary>      
        private int GetBlockCount(int blockSize, long sizefile)
        {
            return (int)Math.Ceiling((double)sizefile / blockSize);
        }
    }
}
