using GZipTest.Helpers;
using GZipTest.Implementations;
using System;
using System.IO;

namespace GZipTest
{
    internal class Program
    {
        //размер считываемого блока
        private static int _blockSize = 1024 * 1024;
        //установим максимальное кол-во блоков для одного контейнера       
        private static int _borderCapacity = 100;
        private static int _countProccessingThread => Environment.ProcessorCount;

        private static int Main(string[] args)
        {
            StartedMessage();           
           int result = 1;
            if (ArgumentsValidator.Validate(args) && MemoryValidator.ValidateMemory(_blockSize, _borderCapacity, _countProccessingThread))
            {
                using (var archiver = AbstractArchiver.CreateArchiver(args[0], args[1], args[2], _blockSize, _borderCapacity, _countProccessingThread))
                {
                    if (archiver.Start())
                    {
                        FinishMessage("Success");
                        result = 0;
                    }
                    else
                    {
                        ClearData(args[2]);
                        FinishMessage("Failure");
                    }
                }
            }
            Console.WriteLine(" Push any key to continue..");
            Console.ReadKey();
            return result;
        }

        private static void ClearData(string outputFilePath)
        {
            if (File.Exists(outputFilePath))
                File.Delete(outputFilePath);
        }

        private static void StartedMessage()
        {
            Console.WriteLine("Set params: GZipTest.exe [compress\\decompress] [Path to input file] [Path to output file]");
            Console.WriteLine("For example: GZipTest.exe compress D:\\input.txt D:\\output.gz");
        }

        private static void FinishMessage(string message)
        {
            Console.WriteLine($"\n {message}");
        }
    }
}
