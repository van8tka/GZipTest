using System;

namespace GZipTest 
{
    internal class ProgressInfo
    {
        private static int persent = 100;
        internal static void Output(int position, int length)
        {          
            int currentPersent =  (position * persent / length);          
            Console.Write($"\r progress: {currentPersent} %");          
        }

        internal static void End()
        {
            Console.Write($"\r progress: {persent} %");
        }
    }
}
