using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public interface IConverter
    {
        void AddBytes(object obj, byte[] bytes, ref int index);
        object GetObject(byte[] bytes, ref int index);
        Type GetType(byte[] bytes, int index);
    }

    public interface IConverterProvider
    {
        IConverter GetConverter(Type type, out short typeId);
        IConverter GetConverter(short typeId);

        void AddConverter<T>(IConverter converter, short typeId);
        void AddConverter(IConverter converter, Type type, short typeId);
    }


}
