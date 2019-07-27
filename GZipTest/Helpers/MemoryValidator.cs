using System;
using System.Diagnostics;
using System.Management;

namespace GZipTest.Helpers
{
    /// <summary>
    /// по дефолту минимально установленную границу кол-ва блоков ставим равной: BorderCapacity = 10
    /// вычисляем минимальный необходимый размер оперативной памяти для работы приложения исходя из BorderCapacity
    /// получаем доступную физическую память, и если физической памяти не хватает, то выводим сообщение о несоответствии конфигурации пк
    /// далее если физ. памяти хватает проверяем доступную оперативную память, если её нехватает выводим сообщение о недостаточном кол-ве RAM
    /// если опер.памяти в достатке, то увеличиваем по возможности границу кол-ва блоков в соответствии с кол-вом свободной оперативной памяти, оставляя 30% свободной
    /// </summary>
    public class MemoryValidator
    {
        /// <summary>
        /// 30% оставляем свободной оперативной памяти
        /// </summary>
        private static float FreeMemoryPersent = 0.3f;
      
        public static bool ValidateMemory(int blockSize, ref int borderCapacity)
        {
            try
            {
                byte helpersDataAmount = 16;
                int fullBlockSize = blockSize + helpersDataAmount;
                var neededRam = CalculateMinimumNeededRam(fullBlockSize, borderCapacity);
                var configRam = GetTotalMemory();
                if (neededRam > configRam)
                    throw new Exception("The configuration of your computer is not available to run this program because the necessary amount of RAM is not installed.");
                var freeRam = GetFreeMemory();
                if (neededRam > freeRam)
                    throw new Exception("You don't have free RAM, try to close some applications to free RAM..");
                //если необходимое кол-во памяти меньше чем 70% свободной оперативной памяти, то пересчитаем границу кол-ва блоков для эффективного использования оперативной памяти               
                borderCapacity = SetAvailableBorderCapacity(freeRam, fullBlockSize, neededRam, borderCapacity);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// устанавливаем границу кол-ва блоков для наиболее полного использования свободной оперативной памяти
        /// </summary>       
        public static int SetAvailableBorderCapacity(long freeRam, int fullBlockSize, long neededRam, int border)
        {
            checked
            {
                try
                {
                    //оставляем 30% памяти свободной
                    long freeRamWithoutSaveSpace = (long)(freeRam - freeRam * FreeMemoryPersent);
                    if (neededRam > freeRamWithoutSaveSpace)
                        return border;
                    //25% запас для работы приложения GZipTest
                    long ramForBlocks = (long)(freeRamWithoutSaveSpace - freeRamWithoutSaveSpace * FreeMemoryPersent);
                    //всего кол-во блоков
                    long allCountBlocksAvailable = ramForBlocks / fullBlockSize;
                    //возвращаем границу количества блока для одного контейнера
                    int newBorderCapacity = (int)allCountBlocksAvailable / 2;
                    if (newBorderCapacity < border)
                        return border;
                    else
                        return newBorderCapacity;
                }
                catch (OverflowException e)
                {
                    Console.WriteLine("Total memory is too large, error overflow type."+e);
                    throw;
                }
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
                    long memoryForContainers = borderCapacity * blockSize * 2;
                    //30% для работы приложения
                    memoryForContainers += (long)(memoryForContainers * FreeMemoryPersent);
                    return memoryForContainers;
                }
                catch (OverflowException e)
                {
                    Console.WriteLine("Amount of needed memory is too large, error overflow type."+e);
                    throw;
                }
            }
        }
    }
}
