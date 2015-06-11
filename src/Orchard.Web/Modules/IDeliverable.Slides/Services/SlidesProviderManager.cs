using System;
using System.Collections.Generic;
using System.Linq;
using IDeliverable.Slides.Providers;
using Orchard.ContentManagement;

namespace IDeliverable.Slides.Services
{
    public class SlidesProviderManager : ISlidesProviderManager
    {
        private readonly Lazy<IEnumerable<ISlidesProvider>> _providers;

        public SlidesProviderManager(Lazy<IEnumerable<ISlidesProvider>> providers)
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
    }
}