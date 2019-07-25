using System;
using System.Diagnostics;
using System.Management;

namespace GZipTest.Helpers
{
    public class MemoryValidator
    {
        private static long MemoryForApp = 1024 * 1024 * 1024;

        public static bool ValidateMemory(int blockSize, int borderCapacity)
        {
            try
            {
                var neededRam = CalculateNeededRam(blockSize, borderCapacity);
                var configRam = GetTotalMemory();
                if (neededRam > configRam)
                    throw new Exception("The configuration of your computer is not available to run this program because the necessary amount of RAM is not installed.");
                var freeRam = GetFreeMemory();
                if (neededRam > freeRam)
                    throw new Exception("You don't have free RAM, try to close some applications to free RAM..");
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }           
        }

        private static long GetTotalMemory()
        {
            var query =  "SELECT Capacity FROM Win32_PhysicalMemory";
            var managementObject = new ManagementObjectSearcher(query);
            long total = 0;
            foreach (var item in managementObject.Get())
            {
                total += Convert.ToInt64(item.Properties["Capacity"].Value);
            }            
            return total*1024*1024;
        }

        private static long GetFreeMemory()
        {
            var ramCounter = new PerformanceCounter("Memory", "Available Bytes");
            return (long)ramCounter.NextValue();
        }

        /// <summary>
        /// расчет необходимой памяти для нормальной работы программы
        /// </summary>
        /// <param name="borderCapacity">верхняя граница количества сохраняемых блоков</param>
        /// <param name="blockSize">размер блока</param>
        /// <returns></returns>
        private static long CalculateNeededRam(int blockSize, int borderCapacity)
        {
            //умножаем на 2 - т.к. два контейнера для считанных данных и для обработанных(gzip) данных
            long memoryForContainers = borderCapacity * blockSize * 2;
            memoryForContainers += MemoryForApp;
            return memoryForContainers;
        }
    }
}
