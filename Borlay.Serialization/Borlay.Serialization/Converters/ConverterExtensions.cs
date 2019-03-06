using Borlay.Serialization.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public static class ConverterExtensions
    {
        public static byte[] ToBytes(this IConverter converter, object obj, ushort bufferSize = 1024)
        {
            var buffer = new byte[bufferSize];
            var index = 0;
            converter.AddBytes(obj, buffer, ref index);
            var bytes = new byte[index];
            Array.Copy(buffer, bytes, index);
            return bytes;
        }

        public static object GetValue(this IConverter converter, byte[] bytes, ref int index)
        {
            return converter.GetObject(bytes, ref index);
        }
    }
}
