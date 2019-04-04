using Borlay.Arrays;
using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public class ByteValueConverter : IConverter, IArrayConverter
    {
        public void AddBytes(object obj, byte[] bytes, ref int index)
        {
            bytes[index++] = (byte)obj;
        }

        public object GetObject(byte[] bytes, ref int index)
        {
            return bytes[index++];
        }

        public void AddArrayBytes(object obj, byte[] bytes, ref int index)
        {
            var array = (byte[])obj;
            var length = array.Length;

            bytes.AddBytes<int>(length, 4, ref index);
            Buffer.BlockCopy(array, 0, bytes, index, length);
            index += length;
        }

        public object GetArrayObject(byte[] bytes, ref int index)
        {
            int length = bytes.GetValue<int>(4, ref index);

            if ((bytes.Length - length) < index) throw new IndexOutOfRangeException("Byte array length is less than needed");

            var array = new byte[length];
            Buffer.BlockCopy(bytes, index, array, 0, array.Length);
            index += array.Length;

            return array;
        }

        public Type GetType(byte[] bytes, int index)
        {
            return typeof(byte);
        }
    }

    public class BoolValueConverter : IConverter
    {
        public void AddBytes(object obj, byte[] bytes, ref int index)
        {
            bytes[index++] = (byte)(((bool)obj) ? 1 : 0);
        }

        public object GetObject(byte[] bytes, ref int index)
        {
            return bytes[index++] == 1 ? true : false;
        }

        public Type GetType(byte[] bytes, int index)
        {
            return typeof(bool);
        }
    }
}
