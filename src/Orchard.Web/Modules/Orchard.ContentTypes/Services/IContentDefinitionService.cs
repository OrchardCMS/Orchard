using System.Collections.Generic;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentTypes.Services {
    public interface IContentDefinitionService : IDependency {
        IEnumerable<ContentTypeDefinition> GetTypeDefinitions();
        ContentTypeDefinition GetTypeDefinition(string name);
        ContentTypeDefinition AddTypeDefinition(string displayName);
        void AlterTypeDefinition(ContentTypeDefinition contentTypeDefinition);
        void RemoveTypeDefinition(string name);

        IEnumerable<ContentPartDefinition> GetPartDefinitions();
        ContentPartDefinition GetPartDefinition(string name);
        ContentPartDefinition AddPartDefinition(string name);
        void AlterPartDefinition(ContentPartDefinition contentPartDefinition);
        void RemovePartDefinition(string name);

        IEnumerable<ContentFieldInfo> GetFieldDefinitions();
    }
}