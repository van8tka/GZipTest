using GZipTest_1.Implementations;

namespace GZipTest_1
{
    class Program
    {
        static int Main(string[] args)
        {
            if (ArgumentsValidator.Validate(args))
            {
                var archiver = AbstractArchiver.CreateArchiver(args[0], args[1], args[2]);
                return archiver.Start() ? 0 : 1;
            }
            else
                return 1;
        }
    }
}
