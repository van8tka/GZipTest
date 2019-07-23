﻿namespace GZipTest.Interfaces
{
    internal interface IArchiver
    {
        /// <summary>
        /// запуск работы архиватора
        /// </summary>
        /// <returns>true - работа завершена без ошибок, false - в процессе работы произошла ошибка</returns>
        bool Start();                  
    }
}
