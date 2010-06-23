using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.Drivers.FieldStorage {
    public interface IFieldStorageProviderSelector : IDependency {
        IFieldStorageProvider GetProvider(ContentPartDefinition.Field partFieldDefinition);
    }
}