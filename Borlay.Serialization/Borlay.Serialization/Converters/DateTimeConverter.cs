using Borlay.Arrays;
using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public class DateTimeConverter : IConverter
    {
        public bool IsInheritable => false;

        public void AddBytes(object obj, byte[] bytes, ref int index)
        {
            var dateTime = (DateTime)obj;
            var value = dateTime.ToBinary();
            bytes.AddBytes<long>(value, 8, ref index);
        }

        public object GetObject(byte[] bytes, ref int index)
        {
            var value = bytes.GetValue<long>(8, ref index);
            var dateTime = DateTime.FromBinary(value);
            return dateTime;
        }

        public Type GetType(byte[] bytes, ref int index)
        {
            return typeof(DateTime);
        }

        public void AddType(Type type, byte[] bytes, ref int index)
        {
            // do nothing
        }
    }
}
