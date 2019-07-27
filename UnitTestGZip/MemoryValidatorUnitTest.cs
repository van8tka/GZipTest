using GZipTest.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestGZip
{
    [TestClass]
   public class MemoryValidatorUnitTest
    {
        [TestMethod]
        public void ValidateMemory_SetBorderCapacityAndBlockSize_ReturnTrue()
        {
            //arrange
            int blockSize = 1024 * 1024;
            int borderCapacity = 1;
            //act
            var result = MemoryValidator.ValidateMemory(blockSize, ref borderCapacity);
            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValidateMemory_SetMaxBorderCapacityAndBlockSize_ReturnFalse()
        {
            //arrange
            int blockSize = 1024 * 1024;
            int borderCapacity = 10000;
            //act
            var result = MemoryValidator.ValidateMemory(blockSize, ref borderCapacity);
            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SetAvailableBorderCapacity_SetEquallFreeRam_DefaultBorderCapacity()
        {
            int fullBlockSize = 10;
            long freeRam = 250;
            int borderCapacity = 10;
            //calculate neededram = 250
            long neededRam = MemoryValidator.CalculateMinimumNeededRam(fullBlockSize, borderCapacity);
            var result = MemoryValidator.SetAvailableBorderCapacity(freeRam, fullBlockSize, neededRam, borderCapacity);
            //border not change
            Assert.IsTrue(result == borderCapacity);
        }
        [TestMethod]
        public void SetAvailableBorderCapacity_SetBigFreeRam_NewBorderCapacity()
        {
            int fullBlockSize = 10;
            long freeRam = 500;
            int borderCapacity = 10;
            //calculate neededram = 250
            long neededRam = MemoryValidator.CalculateMinimumNeededRam(fullBlockSize, borderCapacity);
            var result = MemoryValidator.SetAvailableBorderCapacity(freeRam, fullBlockSize, neededRam, borderCapacity);
            //border changes
            Assert.IsTrue(result > borderCapacity);
        }
        [TestMethod]
        public void SetAvailableBorderCapacity_SetSmallFreeRam_DefaultBorderCapacity()
        {
            int fullBlockSize = 10;
            long freeRam = 350;
            int borderCapacity = 10;
            //calculate neededram = 250
            long neededRam = MemoryValidator.CalculateMinimumNeededRam(fullBlockSize, borderCapacity);
            var result = MemoryValidator.SetAvailableBorderCapacity(freeRam, fullBlockSize, neededRam, borderCapacity);
            //border changes
            Assert.IsTrue(result == borderCapacity);
        }
        [TestMethod]
        public void SetAvailableBorderCapacity_SetMaximumFreeRam_borderCapacity()
        {
            int fullBlockSize = 1024*1024+16;
            long freeRam = 68719476736;                  
            int borderCapacity = 10;
            //calculate neededram = 250
            long neededRam = MemoryValidator.CalculateMinimumNeededRam(fullBlockSize, borderCapacity);
            var result = MemoryValidator.SetAvailableBorderCapacity(freeRam, fullBlockSize, neededRam, borderCapacity);
            //border changes
            Assert.IsTrue(result > borderCapacity);
        }
    }
}
