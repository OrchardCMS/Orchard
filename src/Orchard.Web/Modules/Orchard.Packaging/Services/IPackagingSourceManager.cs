using System;
using System.Collections.Generic;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.Services {
    public interface IPackagingSourceManager : IDependency {
        IEnumerable<PackagingSourceRecord> GetSources();
        void AddSource(string feedTitle, string feedUrl);
        void RemoveSource(int id);

        IEnumerable<PackagingEntry> GetModuleList(PackagingSourceRecord packagingSource = null);
        IEnumerable<PackagingEntry> GetThemeList(PackagingSourceRecord packagingSource = null);
    }
}