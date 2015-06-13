using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Drivers;

namespace IDeliverable.Slides.Services
{
    public class SlidesProviderImportContext
    {
        public SlidesProviderImportContext(ISlidesProvider provider, XElement element, IStorage storage, IContentImportSession session, IContent content)
        {
            Provider = provider;
            Element = element;
            Storage = storage;
            Session = session;
            Content = content;
        }

        public ISlidesProvider Provider { get; private set; }
        public XElement Element { get; private set; }
        public IStorage Storage { get; private set; }
        public IContentImportSession Session { get; private set; }
        public IContent Content { get; private set; }

        public ContentItem GetItemFromSession(string identity)
        {
            return Session.GetItemFromSession(identity);
        }
    }
}