﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public interface ISerializer : IConverter, IConverterProvider
    {
        //byte SerializerType { get; }

    }
}
