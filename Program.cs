using GZipTest_1.Implementations;
using System;

namespace GZipTest_1
{
    class Program
    {    
        static int Main(string[] args)
        {
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
        private static void FinishMessage(string message)
        {
            Console.WriteLine($"\n {message}");        
        }
    }
}
