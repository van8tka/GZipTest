using GZipTest.Helpers;
using GZipTest.Implementations;
using System;
using System.IO;

namespace GZipTest
{
    internal class Program
    {
        //размер считываемого блока
       private static int BlockSize = 1024 * 1024;
        //установим минимальное кол-во блоков для одного контейнера
        //приналичии достаточного кол-ва RAM увеличим это значение
       private static int BorderCapacity = 10;

        private static int Main(string[] args)
        {
            StartedMessage();           
           int result = 1;
            if (ArgumentsValidator.Validate(args) && MemoryValidator.ValidateMemory(BlockSize,ref BorderCapacity))
            {
                using (var archiver = AbstractArchiver.CreateArchiver(args[0], args[1], args[2], BlockSize, BorderCapacity))
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
