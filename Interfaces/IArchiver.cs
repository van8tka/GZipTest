namespace GZipTest_1.Interfaces
{
    internal interface IArchiver: IReadWrite
    {
            bool Start();                  
    }

    internal interface IReadWrite
    {
        void ReadData();
        void WriteData();
    }
}
