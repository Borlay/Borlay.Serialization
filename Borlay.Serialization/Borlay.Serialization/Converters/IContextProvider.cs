using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public interface IContextProvider
    {
        ConverterContext GetContext(Type type);
        ConverterContext GetContext(short typeId);

        bool Contains<T>();
        bool Contains(Type type);

        void AddContext<T>(ConverterContext converterContext, short typeId);
        void AddContext(ConverterContext converterContext, Type type, short typeId);

        void Clear();
    }

    public class ContextProvider : IContextProvider
    {
        protected Dictionary<short, ConverterContext> contexts = new Dictionary<short, ConverterContext>();
        protected Dictionary<Type, short> contextTypes = new Dictionary<Type, short>();

        public virtual bool Contains<T>()
        {
            return Contains(typeof(T));
        }

        public virtual bool Contains(Type type)
        {
            return contextTypes.ContainsKey(type);
        }

        public virtual void AddContext<T>(ConverterContext converterContext, short typeId)
        {
            AddContext(converterContext, typeof(T), typeId);
        }

        public virtual void AddContext(ConverterContext converterContext, Type type, short typeId)
        {
            contextTypes[type] = typeId;
            contexts[typeId] = converterContext;
        }

        public virtual ConverterContext GetContext(Type type)
        {
            return contexts[contextTypes[type]];
        }

        public virtual ConverterContext GetContext(short typeId)
        {
            return contexts[typeId];
        }

        public virtual void Clear()
        {
            contextTypes.Clear();
            contexts.Clear();
        }
    }
}
