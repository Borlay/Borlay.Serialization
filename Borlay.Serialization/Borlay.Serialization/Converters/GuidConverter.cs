using Borlay.Arrays;
using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public class GuidConverter : IConverter
    {
        public void AddBytes(object obj, byte[] bytes, ref int index)
        {
            var guid = (Guid)obj;
            bytes.AddBytes(guid.ToByteArray(), ref index);
        }

        public object GetObject(byte[] bytes, ref int index)
        {
            var gb = new byte[16];
            Buffer.BlockCopy(bytes, index, gb, 0, gb.Length);
            index += gb.Length;
            return new Guid(gb);
        }

        public void AddType(Type type, byte[] bytes, ref int index)
        {
        }

        public Type GetType(byte[] bytes, ref int index)
        {
            return typeof(Guid);
        }
    }
}
