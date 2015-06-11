using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace IDeliverable.Slides.Services
{
    public interface ISlidesProvider : IDependency
    {
        string Name { get; }
        LocalizedString DisplayName { get; }
        string Prefix { get; }
        dynamic BuildEditor(dynamic shapeFactory, IStorage storage, dynamic context = null);
        dynamic UpdateEditor(dynamic shapeFactory, IStorage storage, IUpdateModel updater, dynamic context = null);
        IEnumerable<dynamic> BuildSlides(dynamic shapeFactory, IStorage storage);
        int Priority { get; }
    }
}