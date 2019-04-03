using Borlay.Arrays;
using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public class EnumConverter : IConverter
    {
        private readonly static int size;

        static EnumConverter()
        {
            size = TypeSize.SizeOf<ushort>();
        }

        public void AddBytes(object obj, byte[] bytes, ref int index)
        {
            AddBytes(bytes, obj, ref index);
        }

        public static void AddBytes(byte[] bytes, object obj, ref int index)
        {
            var value = (int)obj;
            if (value > ushort.MaxValue || value < ushort.MinValue)
                throw new Exception($"Enum value is out of range. '{value}'");

            bytes.AddBytes<ushort>((ushort)value, size, ref index);
        }

        public object GetObject(byte[] bytes, ref int index)
        {
            var value = ByteArrayExtensions.GetValue<ushort>(bytes, size, ref index);
            return value;
        }

        public Type GetType(byte[] bytes, int index)
        {
            return typeof(Enum);
        }
    }
}
