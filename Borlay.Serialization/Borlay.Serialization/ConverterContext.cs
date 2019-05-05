using Borlay.Serialization.Notations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Borlay.Serialization
{
    public class ConverterContext
    {
        public Type Type { get; set; }

        public short TypeId { get; set; }

        public int SpaceId { get; set; }

        public DataAttribute Data { get; set; }

        public PropertyContext[] Properties { get; set; }
    }

    public class PropertyContext
    {
        public IncludeAttribute Include { get; set; }

        public ArrayAttribute Array { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public IConverter Converter { get; set; }
    }
}
