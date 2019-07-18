using GZipTest.Implementations;
using System;

namespace GZipTest
{
    class Program
    {    
        static int Main(string[] args)
        {
            StartedMessage();
            int result = 1;
            if (ArgumentsValidator.Validate(args))
            {
                using (var archiver = AbstractArchiver.CreateArchiver(args[0], args[1], args[2]))
                {
                    if (archiver.Start())
                    {
                        FinishMessage("Success");
                        result = 0;
                    }
                    else
                    {
                        FinishMessage("Failure");
                    }
                }                  
            }
            Console.WriteLine(" Push any key to continue..");
            Console.ReadKey();
            return result;
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
