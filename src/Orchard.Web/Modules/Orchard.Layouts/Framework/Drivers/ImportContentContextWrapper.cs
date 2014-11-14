using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Layouts.Framework.Drivers {
    public class ImportContentContextWrapper : IContentImportSession {
        private readonly ImportContentContext _context;

        public ImportContentContextWrapper(ImportContentContext context) {
            _context = context;
        }

        public ContentItem GetItemFromSession(string id) {
            return _context.GetItemFromSession(id);
        }
    }
}