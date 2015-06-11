using System.Collections.Generic;
using IDeliverable.Slides.Providers;
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
        dynamic BuildEditor(dynamic shapeFactory, SlidesProviderContext context);
        dynamic UpdateEditor(dynamic shapeFactory, SlidesProviderContext context, IUpdateModel updater);
        IEnumerable<dynamic> BuildSlides(dynamic shapeFactory, SlidesProviderContext context);
        int Priority { get; }
    }
}