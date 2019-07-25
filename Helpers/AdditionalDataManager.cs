using System;

namespace GZipTest.Helpers
{
    public class AdditionalDataManager 
    {
       
        public byte[] AddedHelpersDataToByteArray(long data, byte[] bytes)
        {
            byte[] dataBytes = BitConverter.GetBytes(data);
            var resultBytes = new byte[dataBytes.Length + bytes.Length];
            dataBytes.CopyTo(resultBytes, 0);
            bytes.CopyTo(resultBytes, dataBytes.Length);
            return resultBytes;
        }

        public byte[] GetHelpersDataFromByteArray(byte[] bytes, out long data)
        {
            int dataLenght = 8;
            byte[] bytesResult = new byte[bytes.Length - dataLenght];
            Array.Copy(bytes, dataLenght, bytesResult, 0, bytesResult.Length);
            byte[] tempBytes = { bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7] };
            data = BitConverter.ToInt64(tempBytes, 0);
            return bytesResult;
        }
    }
}
