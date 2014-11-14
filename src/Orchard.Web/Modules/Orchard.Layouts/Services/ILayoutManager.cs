using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.Services {
    public interface ILayoutManager : IDependency {
        IEnumerable<LayoutPart> GetTemplates();
        LayoutPart GetLayout(int id);
        IEnumerable<IElement> LoadElements(ILayoutAspect layout);
        LayoutDocument BuildDocument(ILayoutAspect layout);
        dynamic RenderLayout(ILayoutAspect layout, string state = null, string displayType = null);

        /// <summary>
        /// Updates the specified layout with the specified template layout.
        /// </summary>
        /// <returns>Returns the merged layout state.</returns>
        string ApplyTemplate(LayoutPart layout, LayoutPart templateLayout);

        /// <summary>
        /// Updates the specified layout with its selected template layout.
        /// </summary>
        /// <returns>Returns the merged layout state.</returns>
        string ApplyTemplate(LayoutPart layout);

        /// <summary>
        /// Updates the specified layout with the specified template layout.
        /// </summary>
        /// <returns>Returns the merged layout state.</returns>
        string ApplyTemplate(IEnumerable<IElement> layout, IEnumerable<IElement> templateLayout);
        
        /// <summary>
        /// Updates the specified layout by unmarking all templated elements so that they become normal elements again.
        /// </summary>
        string DetachTemplate(IEnumerable<IElement> elements);

        IEnumerable<LayoutPart> GetTemplateClients(int templateId, VersionOptions versionOptions);
        IEnumerable<IElement> CreateDefaultLayout();
    }
}