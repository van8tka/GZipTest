using GZipTest_1.Implementations;
using System;
using System.Diagnostics;
using System.IO;

namespace GZipTest_1
{
    class Program
    {

       // public static string inputFile = "D:\\Games\\intest3.pdf";
        public static string inputFile = "..\\..\\intest2.mp4";
        public static string ouputAfterCompress = "..\\..\\outputAfterCompress.mp4";      
        public static string outputFile = "..\\..\\outputTest.gz";
       
        static int Main(string[] args)
        {          
            SetPath();
           // var archiver = new Compression(inputFile, outputFile);
              var archiver = new Decompression(outputFile, ouputAfterCompress);
           int i = archiver.Start() ? 0 : 1;
           return i;
        }

        private static void SetPath()
        {          
            var environment = Environment.CurrentDirectory;          
            inputFile = Path.Combine(environment, inputFile);
            outputFile = Path.Combine(environment, outputFile);
         //   RecreateFile(outputFile);            
            ouputAfterCompress = Path.Combine(environment, ouputAfterCompress);
            RecreateFile(ouputAfterCompress);
        }

        private static void RecreateFile(string file)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
                File.Create(file).Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                Debugger.Break();
            }
        }
    }
}
