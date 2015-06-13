using System.Collections.Generic;
using System.Xml.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Drivers;

namespace IDeliverable.Slides.Services
{
    public interface ISlidesProviderService : IDependency
    {
        IEnumerable<ISlidesProvider> GetProviders();
        ISlidesProvider GetProvider(string name);
        IEnumerable<dynamic> BuildEditors(dynamic shapeFactory, SlidesProviderContext context);
        IEnumerable<dynamic> UpdateEditors(dynamic shapeFactory, SlidesProviderContext context, IUpdateModel updater);
        XElement Export(IStorage storage, IContent content);
        void Import(IStorage storage, XElement element, IContentImportSession context, IContent content);
    }
}