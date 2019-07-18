namespace GZipTest_1.Interfaces
{
    internal interface IDecompression
    {
        /// <summary>
        /// метод декомпрессии(разархивации) блока данных
        /// </summary>
        /// <param name="indexThread">индекс потока</param>
        void DecompressData(object indexThread);
    }
}
