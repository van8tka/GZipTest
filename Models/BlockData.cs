namespace GZipTest.Models
{

    internal struct BlockData
    {
        internal int Number { get; private set; }
        internal byte[] Bytes { get; private set; }

        internal BlockData(int number, byte[] bytes)
        {
            Number = number;
            Bytes = bytes;
        }
    }
}
