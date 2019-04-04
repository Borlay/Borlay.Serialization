using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public interface ISerializer : IConverter
    {
        IContextProvider ContextProvider { get; }
        IConverterProvider ConverterProvider { get; }

        void Register<T>();
        void Register(Type type);
    }
}
