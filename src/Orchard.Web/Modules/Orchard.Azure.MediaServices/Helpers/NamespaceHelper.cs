using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Orchard.Azure.MediaServices.Helpers {

    public class NamespaceHelper {

        public static XmlNamespaceManager CreateNamespaceManager(XElement xml) {
            var nav = xml.CreateNavigator();
            var nsm = new XmlNamespaceManager(nav.NameTable);
            nsm.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            nsm.AddNamespace("me", "http://schemas.microsoft.com/windowsazure/mediaservices/2013/05/mediaencoder/metadata");

            return nsm;
        }

        public static XmlNamespaceManager CreateNamespaceManager(XDocument xml) {
            var reader = xml.CreateReader();
            var nsm = new XmlNamespaceManager(reader.NameTable);
            nsm.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            nsm.AddNamespace("me", "http://schemas.microsoft.com/windowsazure/mediaservices/2013/05/mediaencoder/metadata");

            return nsm;
        }
    }
}
