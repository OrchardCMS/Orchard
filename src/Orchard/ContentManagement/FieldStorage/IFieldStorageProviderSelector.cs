using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.FieldStorage {
    public interface IFieldStorageProviderSelector : IDependency {
        IFieldStorageProvider GetProvider(ContentPartDefinition.Field partFieldDefinition);
    }
}