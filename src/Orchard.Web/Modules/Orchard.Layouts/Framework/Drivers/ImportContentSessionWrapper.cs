using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Layouts.Framework.Drivers {
    public class ImportContentSessionWrapper : IContentImportSession {
        private readonly ImportContentSession _session;

        public ImportContentSessionWrapper(ImportContentSession session) {
            _session = session;
        }

        public ContentItem GetItemFromSession(string id) {
            return _session.Get(id);
        }
    }
}