using System;
using System.IO;
using GZipTest.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestGZip
{
    [TestClass]
    public class CompressionUnitTest
    {
        private AbstractArchiver _archiver;
        private string _inputPath;
        private string _outputPath;

        [TestInitialize]
        public void Setup()
        {
            _inputPath = Path.Combine(Environment.CurrentDirectory, "inputTest.txt");
            _outputPath = Path.Combine(Environment.CurrentDirectory, "outputTest.gz");
            CreateInputFile(_inputPath);
            File.Create(_outputPath).Close();
            int blockSize = 1;
            int boundedCapacity = 10;
            _archiver = AbstractArchiver.CreateArchiver("compress", _inputPath, _outputPath, blockSize, boundedCapacity);
        }

        private void CreateInputFile(string path)
        {
            using (var stream = File.CreateText(path))
            {
                stream.WriteLine("hello world");
            }
        }

        [TestMethod]
        public void Start_ValidCompressionData_True()
        {
           var result = _archiver.Start();
           Assert.IsTrue(result);
        }
 
        [TestCleanup]
        public void TearDown()
        {
            RemoveFiles(_inputPath);
            RemoveFiles(_outputPath);
        }

        private void RemoveFiles(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
