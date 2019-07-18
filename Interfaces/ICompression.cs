namespace GZipTest_1.Interfaces
{
    internal interface ICompression
    {
        /// <summary>
        /// метод сжатия блока данных
        /// </summary>
        /// <param name="indexThread">индекс потока</param>
        void CompressData(object indexThread);
    }
}
