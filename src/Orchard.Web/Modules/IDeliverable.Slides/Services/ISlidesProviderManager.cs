using System.Collections.Generic;
using IDeliverable.Slides.Providers;
using Orchard;
using Orchard.ContentManagement;

namespace IDeliverable.Slides.Services
{
    public interface ISlidesProviderManager : IDependency
    {
        IEnumerable<ISlidesProvider> GetProviders();
        ISlidesProvider GetProvider(string name);
        IEnumerable<dynamic> BuildEditors(dynamic shapeFactory, SlidesProviderContext context);
        IEnumerable<dynamic> UpdateEditors(dynamic shapeFactory, SlidesProviderContext context, IUpdateModel updater);
    }
}