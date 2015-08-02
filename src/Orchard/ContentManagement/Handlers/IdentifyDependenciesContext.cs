using System;
using System.Xml.Linq;

namespace Orchard.ContentManagement.Handlers {
    public class IdentifyDependenciesContext {

        public IdentifyDependenciesContext(XElement contentElement, ImportContentSession importContentSession) {
            ContentElement = contentElement;
            ImportContentSession = importContentSession;
        }

        public XElement ContentElement { get; set; }
        private ImportContentSession ImportContentSession { get; }

        public void RegisterDependency(string id) {
            if(!String.IsNullOrWhiteSpace(id))
                ImportContentSession.RegisterDependency(new ContentIdentity(id));
        }
    }
}