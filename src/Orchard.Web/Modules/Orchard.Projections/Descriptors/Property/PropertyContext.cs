using System.Collections.Generic;

namespace Orchard.Projections.Descriptors.Property
{
    public class PropertyContext
    {
        public PropertyContext()
        {
            Tokens = new Dictionary<string, object>();
        }

        public IDictionary<string, object> Tokens { get; set; }
        public dynamic State { get; set; }
    }
}