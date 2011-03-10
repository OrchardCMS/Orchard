namespace Orchard.ImportExport.Services {
    public interface IImportExportService : IDependency {
        void Import(string recipe);
    }
}


