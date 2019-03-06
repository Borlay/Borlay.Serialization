using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public class ArrayConverter : IConverter
    {
        public ArrayConverter()
        {
        }

        public void AddBytes(object obj, byte[] bytes, ref int index)
        {
            throw new NotImplementedException();
        }

        public object GetObject(byte[] bytes, ref int index)
        {
            throw new NotImplementedException();
        }
    }
}
