using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace IDeliverable.Slides.Services
{
    public abstract class SlidesProvider : Component, ISlidesProvider
    {
        public virtual string Name => GetType().Name.Replace("Provider", "");
        public virtual LocalizedString DisplayName => T(Name.CamelFriendly());
        public virtual int Priority => 0;
        public virtual string Prefix => GetType().Name;

        public abstract dynamic BuildEditor(dynamic shapeFactory, SlidesProviderContext context);

        public abstract dynamic UpdateEditor(dynamic shapeFactory, SlidesProviderContext context, IUpdateModel updater);

        public abstract IEnumerable<dynamic> BuildSlides(dynamic shapeFactory, SlidesProviderContext context);

        public virtual void Exporting(SlidesProviderExportContext context)
        {
        }

        public virtual void Importing(SlidesProviderImportContext context)
        {
        }
    }
}