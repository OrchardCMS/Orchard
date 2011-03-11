using System.Collections.Generic;

namespace Orchard.ImportExport.Services {
    public interface IImportExportService : IDependency {
        void Import(string recipeText);
        string Export(IEnumerable<string> contentTypes, bool exportMetadata, bool exportData, bool exportSettings);
    }
}


