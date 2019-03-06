using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Notations
{
    public class DataAttribute : Attribute
    {
        public short TypeId { get; private set; }

        internal bool IsSystem { get; set; }

        public DataAttribute(short typeId)
        {
            //if (type < 100) // todo change
            //    throw new ArgumentOutOfRangeException(nameof(type), "Data types less than 100 are reserved");

            this.TypeId = typeId;
        }
    }
}
