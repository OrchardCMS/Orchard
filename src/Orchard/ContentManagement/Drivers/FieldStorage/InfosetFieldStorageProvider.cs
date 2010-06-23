using System;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.Drivers.FieldStorage {
    public class InfosetFieldStorageProvider : IFieldStorageProvider {
        public string ProviderName {
            get { return FieldStorageProviderSelector.DefaultProviderName; }
        }

        public IFieldStorage BindStorage(ContentPartDefinition.Field partFieldDefinition) {
            throw new NotImplementedException();
        }
    }
}