using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.Services {
    public interface ILayoutManager : IDependency {
        IEnumerable<LayoutPart> GetTemplates();
        LayoutPart GetLayout(int id);
        IEnumerable<Element> LoadElements(ILayoutAspect layout);

        /// <summary>
        /// Renders the specified layout Data into a shape tree.
        /// </summary>
        /// <param name="data">The layout Data.</param>
        /// <param name="displayType">Optional. The dislay type to use when rendering the elements.</param>
        /// <param name="content">Optional. Provides additional context to the elements being loaded and rendered.</param>
        /// <returns>A shape representing the layout to be rendered.</returns>
        dynamic RenderLayout(string data, string displayType = null, IContent content = null);

        /// <summary>
        /// Updates the specified layout with the specified template layout.
        /// </summary>
        /// <returns>Returns the merged layout Data.</returns>
        IEnumerable<Element> ApplyTemplate(LayoutPart layout, LayoutPart templateLayout);

        /// <summary>
        /// Updates the specified layout with its selected template layout.
        /// </summary>
        /// <returns>Returns the merged layout Data.</returns>
        IEnumerable<Element> ApplyTemplate(LayoutPart layout);

        /// <summary>
        /// Updates the specified layout with the specified template layout.
        /// </summary>
        /// <returns>Returns the merged layout Data.</returns>
        IEnumerable<Element> ApplyTemplate(IEnumerable<Element> layout, IEnumerable<Element> templateLayout);
        
        /// <summary>
        /// Updates the specified layout by unmarking all templated elements so that they become normal elements again.
        /// </summary>
        IEnumerable<Element> DetachTemplate(IEnumerable<Element> elements);

        IEnumerable<LayoutPart> GetTemplateClients(int templateId, VersionOptions versionOptions);
        IEnumerable<Element> CreateDefaultLayout();
        void Exporting(ExportLayoutContext context);
        void Importing(ImportLayoutContext context);
    }
}