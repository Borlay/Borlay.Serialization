using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Notations
{
    public class IncludeAttribute : Attribute
    {
        public byte Order { get; private set; }

        public bool IsRequired { get; private set; }

        public IncludeAttribute(byte order, bool isRequired)
        {
            this.Order = order;
            this.IsRequired = isRequired;
        }
    }
}
