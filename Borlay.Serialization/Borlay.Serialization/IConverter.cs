using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization
{
    public interface IConverter
    {
        void AddBytes(object obj, byte[] bytes, ref int index);
        object GetObject(byte[] bytes, ref int index);
        Type GetType(byte[] bytes, ref int index);

        void AddType(Type type, byte[] bytes, ref int index);
    }

    public interface IArrayConverter
    {
        void AddArrayBytes(object obj, byte[] bytes, ref int index);
        object GetArrayObject(byte[] bytes, ref int index);
    }
}
