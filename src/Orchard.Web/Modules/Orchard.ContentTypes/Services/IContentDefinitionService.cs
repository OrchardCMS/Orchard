using System.Collections.Generic;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentTypes.Services {
    public interface IContentDefinitionService : IDependency {
        IEnumerable<ContentTypeDefinition> GetTypeDefinitions();
        ContentTypeDefinition GetTypeDefinition(string name);
        void AddTypeDefinition(ContentTypeDefinition contentTypeDefinition);
        void AlterTypeDefinition(ContentTypeDefinition contentTypeDefinition);
        void RemoveTypeDefinition(string name);

        ContentPartDefinition GetPartDefinition(string name);
        void AddPartDefinition(ContentPartDefinition contentPartDefinition);
        void AlterPartDefinition(ContentPartDefinition contentPartDefinition);
        void RemovePartDefinition(string name);

        IEnumerable<ContentFieldInfo> GetFieldDefinitions();
    }
}