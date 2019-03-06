using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public interface ISerializer : IConverter
    {
        byte SerializerType { get; }

        void AddConverter<T>(IConverter converter, short typeId);
    }
}
