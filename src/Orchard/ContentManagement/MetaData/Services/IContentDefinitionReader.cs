using System.Xml.Linq;
using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.ContentManagement.MetaData.Services {
    /// <summary>
    /// The content definition reader is used to import both content type and content part definitions from a XML format.
    /// </summary>
    public interface IContentDefinitionReader : IDependency {
        /// <summary>
        /// Merges a given content type definition provided in a XML format into a content type definition builder.
        /// </summary>
        /// <param name="element">The XML content type definition.</param>
        /// <param name="contentTypeDefinitionBuilder">The content type definition builder.</param>
        void Merge(XElement element, ContentTypeDefinitionBuilder contentTypeDefinitionBuilder);

        /// <summary>
        /// Merges a given content part definition provided in a XML format into a content part definition builder.
        /// </summary>
        /// <param name="element">The XML content type definition.</param>
        /// <param name="contentPartDefinitionBuilder">The content part definition builder.</param>
        void Merge(XElement element, ContentPartDefinitionBuilder contentPartDefinitionBuilder);
    }
}