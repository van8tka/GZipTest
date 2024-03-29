﻿namespace GZipTest.Models
{

    internal struct BlockData
    {
        internal long SizeFile { get; private set; }
        internal long Position { get; private set; }
        internal byte[] Bytes { get; private set; }

        internal BlockData(long position, long sizeFile, byte[] bytes)
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
