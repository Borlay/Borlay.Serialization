using Borlay.Arrays;
using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public class ValueConverter<T> : IConverter
    {
        private readonly int size;

        public ValueConverter()
        {
            this.size = TypeSize.SizeOf<T>();
        }

        public void AddBytes(object obj, byte[] bytes, ref int index)
        {
            ByteArrayExtensions.AddBytes<T>(bytes, (T)obj, size, ref index);
        }

        public object GetObject(byte[] bytes, ref int index)
        {
            return ByteArrayExtensions.GetValue<T>(bytes, size, ref index);
        }
    }
}
