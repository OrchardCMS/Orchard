using System.Xml.Linq;

namespace Orchard.Packaging {
    static class AtomExtensions {
        public static string Atom(this XElement entry, string localName) {
            var element = entry.Element(AtomXName(localName));
            return element != null ? element.Value : null;
        }

        public static XName AtomXName(string localName) {
            return XName.Get(localName, "http://www.w3.org/2005/Atom");
        }
    }
}