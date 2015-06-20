using Orchard;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Elements;
using Orchard.Localization;

namespace IDeliverable.Slides.Services
{
    public interface ISlideshowPlayerEngine : IDependency
    {
        /// <summary>
        /// The technical name of the engine.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The friendly name of the engine.
        /// </summary>
        LocalizedString DisplayName { get; }

        /// <summary>
        /// A dictionary for engine specific settings.
        /// </summary>
        ElementDataDictionary Data { get; set; }

        /// <summary>
        /// Returns a shape that renders the engine settings UI.
        /// </summary>
        dynamic BuildEditor(dynamic shapeFactory);

        /// <summary>
        /// Updates the engine settings and returns the editor shape.
        /// </summary>
        dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater);

        /// <summary>
        /// Returns a shape that renders the slide show on the front-end.
        /// </summary>
        dynamic BuildDisplay(dynamic shapeFactory);
    }
}