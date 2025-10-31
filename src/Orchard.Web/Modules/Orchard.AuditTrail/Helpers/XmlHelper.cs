using System;
using System.Xml.Linq;

namespace Orchard.AuditTrail.Helpers
{
    public static class XmlHelper
    {
        public static XElement Parse(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return null;

            try
            {
                return XElement.Parse(xml);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}