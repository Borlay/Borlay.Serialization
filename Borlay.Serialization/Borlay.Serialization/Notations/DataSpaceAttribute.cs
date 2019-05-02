using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Notations
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class DataSpaceAttribute : Attribute
    {
        public int SpaceId { get; private set; }

        public DataSpaceAttribute(int spaceId)
        {
            this.SpaceId = spaceId;
        }
    }
}
