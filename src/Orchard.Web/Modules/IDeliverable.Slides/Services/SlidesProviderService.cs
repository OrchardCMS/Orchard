using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Drivers;

namespace IDeliverable.Slides.Services
{
    public class SlidesProviderService : ISlidesProviderService
    {
        private readonly Lazy<IEnumerable<ISlidesProvider>> _providers;

        public SlidesProviderService(Lazy<IEnumerable<ISlidesProvider>> providers)
        {
            _providers = providers;
        }

        public IEnumerable<ISlidesProvider> GetProviders()
        {
            return _providers.Value.OrderByDescending(x => x.Priority);
        }

        public ISlidesProvider GetProvider(string name)
        {
            return GetProviders().SingleOrDefault(x => x.Name == name);
        }

        public IEnumerable<dynamic> BuildEditors(dynamic shapeFactory, SlidesProviderContext context)
        {
            var providers = GetProviders().ToList();
            var editorShapes = providers.Select(x =>
            {
                var shape = x.BuildEditor(shapeFactory, context);
                shape.Provider = x;
                return shape;
            });

            return editorShapes;
        }

        public IEnumerable<dynamic> UpdateEditors(dynamic shapeFactory, SlidesProviderContext context, IUpdateModel updater)
        {
            var providers = GetProviders().ToList();
            var editorShapes = providers.Select(x =>
            {
                var shape = x.UpdateEditor(shapeFactory, context, updater);
                shape.Provider = x;
                return shape;
            });

            return editorShapes;
        }

        public XElement Export(IStorage storage, IContent content)
        {
            var providersElement = new XElement("Providers");
            foreach (var provider in GetProviders())
            {
                var providerExportContext = new SlidesProviderExportContext(provider, new XElement(provider.Name), storage, content);

                provider.Exporting(providerExportContext);
                providersElement.Add(providerExportContext.Element);
            }

            return providersElement;
        }

        public void Import(IStorage storage, XElement element, IContentImportSession context, IContent content)
        {
            if (element == null)
                return;
            
            foreach (var provider in GetProviders())
            {
                var providerElement = element.Element(provider.Name);

                if (providerElement == null)
                    continue;

                var providerImportContext = new SlidesProviderImportContext(provider, providerElement, storage, context, content);
                provider.Importing(providerImportContext);
            }
        }
    }
}