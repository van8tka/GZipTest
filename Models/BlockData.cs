namespace GZipTest.Models
{

    internal struct BlockData
    {
        internal int SizeFile { get; private set; }
        internal int Position { get; private set; }
        internal byte[] Bytes { get; private set; }

        internal BlockData(int position, int sizeFile, byte[] bytes)
        {
            SizeFile = sizeFile;
            Position = position;
            Bytes = bytes;
        }
        internal BlockData(byte[] bytes)
        {
            SizeFile = 0;
            Position = 0;
            Bytes = bytes;
        }
    }
}
