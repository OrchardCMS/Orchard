using System.Xml.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.MetaData.Services {
    /// <summary>
    /// The content definition writer is used to export both content type and content part definitions to a XML format.
    /// </summary>
    public interface IContentDefinitionWriter : IDependency {
        /// <summary>
        /// Exports a content type definition to a XML format.
        /// </summary>
        /// <param name="contentTypeDefinition">The type definition to be exported.</param>
        /// <returns>The content type definition in an XML format.</returns>
        XElement Export(ContentTypeDefinition contentTypeDefinition);

        /// <summary>
        /// Exports a content part definition to a XML format.
        /// </summary>
        /// <param name="contentPartDefinition">The part definition to be exported.</param>
        /// <returns>The content part definition in a XML format.</returns>
        XElement Export(ContentPartDefinition contentPartDefinition);
    }
}
