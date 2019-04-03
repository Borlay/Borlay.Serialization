using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Notations
{
    public class DataAttribute : Attribute
    {
        public short TypeId { get; private set; }

        public bool IsSystem { get; set; }

        public DataAttribute(short typeId, bool isSystem = false)
        {
            //if (type < 100) // todo change
            //    throw new ArgumentOutOfRangeException(nameof(type), "Data types less than 100 are reserved");

            this.IsSystem = isSystem;
            this.TypeId = typeId;
        }
    }
}
