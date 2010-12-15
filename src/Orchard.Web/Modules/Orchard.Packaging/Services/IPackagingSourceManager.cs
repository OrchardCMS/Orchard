using System.Collections.Generic;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.Services {
    public interface IPackagingSourceManager : IDependency {
        IEnumerable<PackagingSource> GetSources();
        void AddSource(string feedTitle, string feedUrl);
        void RemoveSource(int id);

        IEnumerable<PackagingEntry> GetModuleList(PackagingSource packagingSource = null);
        IEnumerable<PackagingEntry> GetThemeList(PackagingSource packagingSource = null);
    }
}