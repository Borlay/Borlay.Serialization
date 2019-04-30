using System;
using System.Collections.Generic;
using System.Text;
using Borlay.Arrays;

namespace Borlay.Serialization.Converters
{
    public class StringConverter : IConverter
    {
        private readonly byte defaultEncoding;
        private readonly Encoding[] encodings;

        public StringConverter()
            : this(Encoding.UTF8)
        {
        }

        public StringConverter(Encoding defaultEncoding)
        {
            encodings = new Encoding[] { Encoding.ASCII, Encoding.UTF7, Encoding.UTF8, Encoding.BigEndianUnicode, Encoding.UTF32 };
            this.defaultEncoding = 255;

            for(int i = 0; i < encodings.Length; i++)
            {
                if (encodings[i] == defaultEncoding)
                    this.defaultEncoding = (byte)i;
            }

            if (this.defaultEncoding == 255)
                throw new NotSupportedException($"Default encoding '{defaultEncoding}' is not supported");
        }

        public void AddBytes(object obj, byte[] bytes, ref int index)
        {
            var value = (string)obj;
            var valueBytes = encodings[defaultEncoding].GetBytes(value);
            if (valueBytes.Length > ushort.MaxValue)
                throw new ArgumentNullException($"String length should not exceed '{ushort.MaxValue}'. Current is '{valueBytes.Length}'");

            bytes[index++] = defaultEncoding;
            bytes.AddBytes<ushort>((ushort)valueBytes.Length, 2, ref index);

            Array.Copy(valueBytes, 0, bytes, index, valueBytes.Length);
            index += valueBytes.Length;
        }

        public object GetObject(byte[] bytes, ref int index)
        {
            var dencoding = bytes[index++];
            var count = bytes.GetValue<ushort>(2, ref index); 

            var value = encodings[dencoding].GetString(bytes, index, count);
            index += count;
            return value;
        }

        public Type GetType(byte[] bytes, ref int index)
        {
            return typeof(string);
        }

        public void AddType(Type type, byte[] bytes, ref int index)
        {
            // do nothing
        }
    }
}
