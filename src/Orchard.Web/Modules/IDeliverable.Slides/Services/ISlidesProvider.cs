using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace IDeliverable.Slides.Services
{
    public interface ISlidesProvider : IDependency
    {
        /// <summary>
        /// The technical name of the provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The friendly name of the provider.
        /// </summary>
        LocalizedString DisplayName { get; }

        /// <summary>
        /// The prefix to use when rendering the editor fields.
        /// </summary>
        string Prefix { get; }

        /// <summary>
        /// Returns a shape that renders the editor UI.
        /// </summary>
        dynamic BuildEditor(dynamic shapeFactory, SlidesProviderContext context);

        /// <summary>
        /// Updates the settings and returns the editor shape.
        /// </summary>
        dynamic UpdateEditor(dynamic shapeFactory, SlidesProviderContext context, IUpdateModel updater);

        /// <summary>
        /// Returns a list of shapes, where each shape represents a slide.
        /// </summary>
        IEnumerable<dynamic> BuildSlides(dynamic shapeFactory, SlidesProviderContext context);

        /// <summary>
        /// The priority value is used to order the available providers by.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Exports the settings to the specified context.
        /// </summary>
        void Exporting(SlidesProviderExportContext context);

        /// <summary>
        /// Imports the settings from the specified context.
        /// </summary>
        /// <param name="context"></param>
        void Importing(SlidesProviderImportContext context);
    }
}