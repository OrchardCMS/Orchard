using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.Core.Contents.Services {
    public interface IContentDefinitionService : IDependency {
        IEnumerable<ContentTypeDefinition> GetTypeDefinitions();
        ContentTypeDefinition GetTypeDefinition(string name);
        void AddTypeDefinition(ContentTypeDefinition contentTypeDefinition);
        void AlterTypeDefinition(ContentTypeDefinition contentTypeDefinition);
        void RemoveTypeDefinition(string name);
    }
}