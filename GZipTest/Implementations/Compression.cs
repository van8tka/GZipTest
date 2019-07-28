using GZipTest.Models;
using System;
using System.IO;
using System.IO.Compression;
 
namespace GZipTest.Implementations
{
    internal class Compression : AbstractArchiver
    {
        //ctor
        internal Compression(string input, string output, int blockSize, int boundedCapacity, int countProccessingThreads) : base(input, output, blockSize, boundedCapacity, countProccessingThreads)
        {
            BlocksCount = GetBlockCount(BlockSize);
        }

        protected override void ReadData()
        {
            try
            {
                using (var input = new FileStream(InputFile, FileMode.Open, FileAccess.Read))
                {
                    var lenght = input.Length;
                    int readCount;
                    byte[] bytes;
                    while (input.Position < lenght && !IsError)
                    {                       
                        if (lenght - input.Position < BlockSize)
                            readCount = (int)(lenght - input.Position);
                        else
                            readCount = BlockSize;
                        bytes = new byte[readCount];
                        input.Read(bytes, 0, readCount);
                        //добавим доп инф. о позиции массива байт в файле и размере файла
                        bytes = AddedHelpersData(input.Position - readCount, lenght, bytes);
                        BlockReaded.AddBlock(new BlockData(bytes));
                    }
                    BlockReaded.Finish();                  
                }
            }
            catch (OutOfMemoryException e)
            {
                ErrorOutput(e,"Not enough RAM to complete file read. Please close other applications and try again. ");                       
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
        /// <summary>
        /// сжатие данных
        /// </summary>        
        protected override void ProccessingData(object indexThread)
        {
            try
            {
                BlockData block;
                while (true && !IsError)
                {                   
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
                        return;
                    }
                }           
            }
            catch(IOException e)
            {
                ErrorOutput(e, "Unable to complete compression operation due to insufficient RAM. Please close other applications and try again. ");                
            }
            catch (OutOfMemoryException e)
            {
                ErrorOutput(e, "Not enough RAM to complete file compression. Please close other applications and try again. ");     
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
                int blocksWrite = 0;
                BlockData block;
                byte[] lenghtOfBlock;
                while (true && !IsError)
                {
                    using (var outputStream = new FileStream(OutputFile, FileMode.Append, FileAccess.Write))
                    {                        
                        if (BlockProcessed.TryTakeBlock(out block))
                        {
                            //получим размер сжатых данных для последующей декомпрессии
                            lenghtOfBlock = BitConverter.GetBytes(block.Bytes.Length);
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
                            return;
                        }
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
        /// получение общего колличества блоков в зависимости от исходного размера входящего файла
        /// </summary>       
        private int GetBlockCount(int blockSize)
        {
            if (string.IsNullOrEmpty(InputFile))
                return 0;
            var file = new FileInfo(InputFile);
            return (int)Math.Ceiling((double)file.Length / blockSize);
        }
        /// <summary>
        /// добавление в массив байт 8 байт несущих доп. инф.: первые 4 байта - позиция массива байт, вторые 4 байта - размер файла до архивации
        /// </summary>       
        /// <returns>массив байт с добавленной доп. информацией 8 байт</returns>
        private byte[] AddedHelpersData(long position, long lenght, byte[] bytes)
        {
            return DataManager.AddedHelpersDataToByteArray(position, DataManager.AddedHelpersDataToByteArray(lenght, bytes));
        }
    }
}