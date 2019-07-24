using System;

namespace GZipTest.Helpers
{
    //todo: MUST REMOVE CLASS before send prodaction
    public class CountBlocks
    {
        bool _isShow;
        internal CountBlocks(bool isShow)
        {
            _isShow = isShow;
        }


        //todo: remove this region
        #region COUNT BLOCKS
        private int CR = 0;
        private int CW = 0;
        private int CC = 0;
        object _lock1 = new object();
        object _lock2 = new object();
        object _lock3 = new object();

        internal void CountBR()
        {
            if(_isShow)
            {
                lock (_lock1)
                    CR++;
                Console.Write($"\r                                                                     Read blocks {CR}");
            }
           
        }

        internal void CountBZ()
        {
            if (_isShow)
            {
                lock (_lock3)
                    CC++;
                Console.Write($"\r                                          Zip blocks {CC}");
            }
        }
        internal void CountBW()
        {
            if (_isShow)
            {
                lock (_lock2)
                    CW++;
                Console.Write($"\r                    Write blocks {CW}");
            }
        }

        #endregion

    }
}
