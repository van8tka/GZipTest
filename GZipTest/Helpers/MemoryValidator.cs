using System;
using System.Diagnostics;
using System.Management;

namespace GZipTest.Helpers
{
   
    public class MemoryValidator
    {
         
        public static bool ValidateMemory(int blockSize, ref int borderCapacity)
        {
            try
            {             
                var neededRam = CalculateMinimumNeededRam(blockSize, borderCapacity);
                var configRam = GetTotalMemory();
                if (neededRam > configRam)
                    throw new Exception("The configuration of your computer is not available to run this program because the necessary amount of RAM is not installed.");
                var freeRam = GetFreeMemory();
                if (neededRam > freeRam)
                    throw new Exception("You don't have free RAM, try to close some applications to free RAM..");              
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private static long GetTotalMemory()
        {
            checked
            {
                try
                {
                    var query = "SELECT Capacity FROM Win32_PhysicalMemory";
                    var managementObject = new ManagementObjectSearcher(query);
                    long total = 0;
                    foreach (var item in managementObject.Get())
                    {
                        total += Convert.ToInt64(item.Properties["Capacity"].Value);
                    }
                    return total * 1024 * 1024;
                }
                catch (OverflowException e)
                {
                    Console.WriteLine("Total memory is too large, error overflow type."+e);
                    throw;
                }
            }
        }

        private static long GetFreeMemory()
        {
            checked
            {
                try
                {
                    var ramCounter = new PerformanceCounter("Memory", "Available Bytes");
                    return (long)ramCounter.NextValue();
                }
                catch (OverflowException e)
                {
                    Console.WriteLine("Available memory is too large, error overflow type."+e);
                    throw;
                }
            }
        }

        /// <summary>
        /// расчет минимально необходимой памяти для нормальной работы программы  
        /// </summary>         
        public static long CalculateMinimumNeededRam(int blockSize, int borderCapacity)
        {
            checked
            {
                try
                {                   
                    //умножаем на 2 - т.к. два контейнера для считанных данных и для обработанных(gzip) данных
                    long memoryForContainers = borderCapacity * GetFullBlockSize(blockSize) * 2;
                    // для работы приложения увеличим необходимую память втрое
                    memoryForContainers = memoryForContainers * 3;
                    return memoryForContainers;
                }
                catch (OverflowException e)
                {
                    Console.WriteLine("Amount of needed memory is too large, error overflow type."+e);
                    throw;
                }
            }
        }
        /// <summary>
        ///    //добавляем 16 байт которые используются для сохранения в блоке позиции данных считанных с файла(8 байт-тип long ) и размера файла(8 байт тип long)  
        /// </summary>
        /// <param name="blockSize">размер блока данных</param>
        /// <returns>размер блока данных с доп. информацией о размере файла и позиции считанных данных</returns>
        private static int GetFullBlockSize(int blockSize)
        {                        
             byte helpersDataAmount = 16;
             return blockSize + helpersDataAmount;           
        }
    }
}
