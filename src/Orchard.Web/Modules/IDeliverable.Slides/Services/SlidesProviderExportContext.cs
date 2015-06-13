using System.Xml.Linq;
using Orchard.ContentManagement;

namespace IDeliverable.Slides.Services
{
    public class SlidesProviderExportContext
    {
        public SlidesProviderExportContext(ISlidesProvider provider, XElement element, IStorage storage, IContent content)
        {
            Provider = provider;
            Element = element;
            Storage = storage;
            Content = content;
        }

        public ISlidesProvider Provider { get; private set; }
        public XElement Element { get; private set; }
        public IStorage Storage { get; private set; }
        public IContent Content { get; private set; }
    }
}