using System;
using System.IO;
using GZipTest.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestGZip
{
    [TestClass]
    public class DecompressionUnitTest
    {
        private string _inputPath;
        private string _outputPath;
        private string _decompressPath;
        private AbstractArchiver _archiver;
        private int blockSize = 1;
        private int boundedCapacity = 10;
        private string _contentFile = "hello world";

        [TestInitialize]
        public void Setup()
        {
            _decompressPath = Path.Combine(Environment.CurrentDirectory, "decompressTest.txt");
            _inputPath = Path.Combine(Environment.CurrentDirectory, "inputTest.txt");
            _outputPath = Path.Combine(Environment.CurrentDirectory, "outputTest.gz");
            File.Create(_decompressPath).Close();
            CreateInputFileAndCompress(_inputPath, _outputPath);                     
            _archiver = AbstractArchiver.CreateArchiver("decompress", _outputPath, _decompressPath, blockSize, boundedCapacity);
        }

        private void CreateInputFileAndCompress(string input, string output)
        {           
                File.Create(_outputPath).Close();
                using (var stream = File.CreateText(input))
                {
                    stream.WriteLine(_contentFile);
                }
                _archiver = AbstractArchiver.CreateArchiver("compress", _inputPath, _outputPath, blockSize, boundedCapacity);
                if (!_archiver.Start())
                    throw new Exception("Error create test data");                            
        }

        [TestMethod]
        public void Start_ValidDecompressioData_True()
        {
            var result = _archiver.Start();
            Assert.IsTrue(result);
            var decompressContent = File.ReadAllText(_decompressPath).Substring(0, _contentFile.Length);
            Assert.AreEqual(_contentFile, decompressContent);
        }

        [TestCleanup]
        public void TearDown()
        {
            RemoveFiles(_inputPath);
            RemoveFiles(_outputPath);
            RemoveFiles(_decompressPath);
        }

        private void RemoveFiles(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
