using GZipTest_1.Interfaces;
using System;
using System.Diagnostics;
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
                using (var input = new FileStream(InputFile, FileMode.Open, FileAccess.Read))
                {
                    var lenght = input.Length;
                    while (input.Position < lenght)
                    {
                        int readCount;
                        if (lenght - input.Position < BlockSize)
                            readCount = (int)(lenght - input.Position);
                        else
                            readCount = BlockSize;
                        var bytes = new byte[readCount];
                        input.Read(bytes, 0, readCount);
                        CountReadBlocks();
                        BlockReaded.TryAdd(bytes);
                        OutputProgress(input.Position, input.Length, "compression");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debugger.Break();
            }
        }


        public void CompressData(object indexThread)
        {
            try
            {
                while (true)
                {
                    byte[] bytes;
                    if (BlockReaded.TryTake(out bytes, 1000))
                    {
                        using (var memStream = new MemoryStream())
                        {
                            using (var gzipStream = new GZipStream(memStream, CompressionMode.Compress))
                            {
                                gzipStream.Write(bytes, 0, bytes.Length);
                            }
                            CountZipBlocks();
                            BlockForWrite.TryAdd(memStream.ToArray());
                        }
                        ManualResetEventArray[(int)indexThread].Set();
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

        //private void AddedExtenstion(Stream stream, string inputFile)
        //{
        //     string extenstion = Path.GetExtension(inputFile);
        //     var bytes = Encoding.Default.GetBytes(extenstion);
        //     stream.Write(bytes, 0, bytes.Length);
        //}

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
                            //получим размер сжатых данных для последующей декомпрессии
                            var lenghtOfBlock = BitConverter.GetBytes(bytes.Length);
                            //запись информации о длине блока для считывания вместо
                            //времени модификации файла в формате Unix (спецификация gzip)
                            lenghtOfBlock.CopyTo(bytes, 4);
                            outputStream.Write(bytes, 0, bytes.Length);
                            CountWriteBlocks();
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
    }
}
