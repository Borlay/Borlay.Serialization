using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public interface IConverterProvider
    {
        IConverter GetConverter(Type type, out short typeId);
        IConverter GetConverter(short typeId);

        void AddConverter<T>(IConverter converter, short typeId);
        void AddConverter(IConverter converter, Type type, short typeId);
    }
}
