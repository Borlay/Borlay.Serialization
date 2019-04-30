using Borlay.Arrays;
using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public class DecimalValueConverter : IConverter
    {
        public void AddBytes(object obj, byte[] bytes, ref int index)
        {
            //byte[] decimalBytes = BitConverterExt.GetBytes(testDecimal);

            var b = BitConverterExt.GetBytes((decimal)obj);
            if (b.Length != 16)
                throw new ArgumentException($"Decimal bytes lenght should be length of 16 but is {b.Length}");

            bytes.AddBytes(b, ref index);
        }

        public object GetObject(byte[] bytes, ref int index)
        {
            var b = new byte[16];
            Buffer.BlockCopy(bytes, index, b, 0, 16);
            index += 16;
            return BitConverterExt.ToDecimal(b);
        }

        public Type GetType(byte[] bytes, ref int index)
        {
            return typeof(decimal);
        }

        public void AddType(Type type, byte[] bytes, ref int index)
        {
            // do nothing
        }
    }


    public class BitConverterExt
    {
        public static byte[] GetBytes(decimal dec)
        {
            //Load four 32 bit integers from the Decimal.GetBits function
            Int32[] bits = decimal.GetBits(dec);
            //Create a temporary list to hold the bytes
            List<byte> bytes = new List<byte>();
            //iterate each 32 bit integer
            foreach (Int32 i in bits)
            {
                var b = BitConverter.GetBytes(i);
                b = ByteArrayExtensions.Endian(b);
                //add the bytes of the current 32bit integer
                //to the bytes list
                bytes.AddRange(b);
            }
            //return the bytes list as an array
            return bytes.ToArray();
        }
        public static decimal ToDecimal(byte[] bytes)
        {
            //check that it is even possible to convert the array
            if (bytes.Length != 16)
                throw new Exception("A decimal must be created from exactly 16 bytes");
            //make an array to convert back to int32's
            Int32[] bits = new Int32[4];
            byte[] b = new byte[4];
            for (int i = 0; i <= 15; i += 4)
            {
                Buffer.BlockCopy(bytes, i, b, 0, 4);
                b = ByteArrayExtensions.Endian(b);

                //convert every 4 bytes into an int32
                bits[i / 4] = BitConverter.ToInt32(b, 0);
            }
            //Use the decimal's new constructor to
            //create an instance of decimal
            return new decimal(bits);
        }
    }
}
