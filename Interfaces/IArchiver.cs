namespace GZipTest_1.Interfaces
{
    internal interface IArchiver: IReadWrite
    {
        /// <summary>
        /// запуск работы архиватора
        /// </summary>
        /// <returns>true - работа завершена без ошибок, false - в процессе работы произошла ошибка</returns>
        bool Start();                  
    }

    internal interface IReadWrite
    {
        /// <summary>
        /// чтение блоков данных из входящего файла
        /// </summary>
        void ReadData();
        /// <summary>
        /// запись блоков данных в результирующий файл
        /// </summary>
        void WriteData();
    }
}
