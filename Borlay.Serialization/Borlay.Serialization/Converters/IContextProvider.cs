using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public interface IContextProvider
    {
        ConverterContext GetContext(Type type);
        ConverterContext GetContext(short typeId);

        void AddContext<T>(ConverterContext converterContext, short typeId);
        void AddContext(ConverterContext converterContext, Type type, short typeId);
    }
}
