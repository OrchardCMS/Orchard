using System.Xml.Linq;
using Orchard.Environment.Extensions;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    internal static class AtomExtensions {
        public static string Atom(this XElement entry, string localName) {
            XElement element = entry.Element(AtomXName(localName));
            return element != null ? element.Value : null;
        }

        public static XName AtomXName(string localName) {
            return XName.Get(localName, "http://www.w3.org/2005/Atom");
        }
    }
}