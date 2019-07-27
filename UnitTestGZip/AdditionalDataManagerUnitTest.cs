using GZipTest.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestGZip
{
    [TestClass]
    public class AdditionalDataManagerUnitTest
    {
        private readonly AdditionalDataManager dataManager;
        public AdditionalDataManagerUnitTest()
        {
            dataManager = new AdditionalDataManager();
        }


        [TestMethod]
        public void AddedHelpersDataToByteArray_SetPositionOrSizeFileAndDefaultBytes_ReturnBytes()
        {
            //arrange
            long position = 1024;            
            var bytes = new byte[124];
            //act
            var result = dataManager.AddedHelpersDataToByteArray(position, bytes);
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(byte[]));
            Assert.AreEqual(4, result[1]);
        }

        [TestMethod]
        public void GetHelpersDataFromByteArray_SetBytesArrayWithOtherData_ReturnBytesAndPosition()
        {
            //arrange
            var bytes = new byte[132];
            long position;
            bytes[1] = 4;
            bytes[8] = 1;
            //act
            var result = dataManager.GetHelpersDataFromByteArray(bytes, out position);
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(byte[]));
            Assert.AreEqual(124, result.Length);
            Assert.AreEqual(1024, position);
        }
      
    }
}
