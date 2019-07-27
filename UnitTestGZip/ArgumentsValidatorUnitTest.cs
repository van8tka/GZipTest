using System;
using System.IO;
using GZipTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestGZip
{
    [TestClass]
    public class ArgumentsValidatorUnitTest
    {
        private string _inputPath { get; set; }
        private string _outputPath { get; set; }
        [TestInitialize]
        public void Setup()
        {
            _inputPath = Path.Combine(Environment.CurrentDirectory, "inputTest.txt");
            _outputPath = Path.Combine(Environment.CurrentDirectory, "outputTest.gz");
            File.Create(_inputPath).Close();
        }

        [TestMethod]
        public void Validate_ExcelentArgs_True()
        {       
            string[] args = { "compress", _inputPath, _outputPath };
            var result = ArgumentsValidator.Validate(args);
            Assert.IsTrue(result);            
        }

        [TestMethod]
        public void Validate_InvalidPathArgs_True()
        {
            _outputPath = "test/test.gz";
            string[] args = { "compress", _inputPath, _outputPath };
            var result = ArgumentsValidator.Validate(args);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_InvalidCountArgs_True()
        {          
            string[] args = { "compress", _inputPath};
            var result = ArgumentsValidator.Validate(args);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_InvalidFirstArgs_True()
        {
            string[] args = { "compressTest", _inputPath, _outputPath };
            var result = ArgumentsValidator.Validate(args);
            Assert.IsFalse(result);
        }

        [TestCleanup]
        public void TearDown()
        {
            RemoveFile(_inputPath);
            RemoveFile(_outputPath);
        }
        private void RemoveFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}

