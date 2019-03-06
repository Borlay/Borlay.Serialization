using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public interface IContextProvider
    {
        ConverterContext GetContext(Type type);
        ConverterContext GetContext(short typeId);
    }
}
