using System;

namespace Orchard.Data
{
    /// <summary>
    /// Marks whether a class should be mapped as an NHibernate record
    /// </summary>
    public class MapAsRecordAttribute : Attribute
    {
        public MapAsRecordAttribute() : this(true) { }

        public MapAsRecordAttribute(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; }
    }
}