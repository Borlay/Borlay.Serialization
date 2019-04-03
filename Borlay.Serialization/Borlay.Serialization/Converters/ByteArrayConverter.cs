using Borlay.Arrays;
using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public class ByteArrayConverter : IConverter
    {
        public void AddBytes(object obj, byte[] bytes, ref int index)
        {
            var byteArray = (ByteArray)obj;
            var addBytes = byteArray.Bytes;
            var length = addBytes.Length;

            if (length > short.MaxValue || length < ushort.MinValue)
                throw new Exception($"Array length should be between '{ushort.MinValue}' and '{short.MaxValue}' but  is '{length}'");

            bytes.AddBytes<ushort>((ushort)length, 2, ref index);
            Buffer.BlockCopy(addBytes, 0, bytes, index, length);
            index += length;
        }

        public object GetObject(byte[] bytes, ref int index)
        {
            ushort length = bytes.GetValue<ushort>(2, ref index);

            if ((bytes.Length - length) < index) throw new IndexOutOfRangeException("Byte array length is less than needed");

            var array = new byte[length];
            Array.Copy(bytes, index, array, 0, array.Length);
            index += array.Length;

            return new ByteArray(array);
        }

        public Type GetType(byte[] bytes, int index)
        {
            return typeof(ByteArray);
        }
    }
}
