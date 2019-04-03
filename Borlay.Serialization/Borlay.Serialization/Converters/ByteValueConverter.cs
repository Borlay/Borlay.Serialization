using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public class ByteValueConverter : IConverter
    {
        public void AddBytes(object obj, byte[] bytes, ref int index)
        {
            bytes[index++] = (byte)obj;
        }

        public object GetObject(byte[] bytes, ref int index)
        {
            return bytes[index++];
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
