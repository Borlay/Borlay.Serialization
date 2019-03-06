using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Notations
{
    public class ArrayAttribute : Attribute
    {
        public short MinLength { get; private set; }

        public short MaxLength { get; private set; }

        public bool OnlyMinOrMax { get; set; }

        public ArrayAttribute(short minLength, short maxLength)
        {
            this.MinLength = minLength;
            this.MaxLength = maxLength;
            this.OnlyMinOrMax = false;
        }
    }
}
