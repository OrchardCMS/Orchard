using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;

namespace IDeliverable.Slides.Services
{
    public interface ISlidesProviderManager : IDependency
    {
        IEnumerable<ISlidesProvider> GetProviders();
        ISlidesProvider GetProvider(string name);
        IEnumerable<dynamic> BuildEditors(dynamic shapeFactory, IStorage storage, dynamic context = null);
        IEnumerable<dynamic> UpdateEditors(dynamic shapeFactory, IStorage storage, IUpdateModel updater, dynamic context = null);
    }
}