﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization
{
    public interface ISerializer : IConverter
    {
        byte Type { get; }

        IContextProvider ContextProvider { get; }
        IConverterProvider ConverterProvider { get; }

        void Register<T>();
        void Register(Type type);
    }
}
