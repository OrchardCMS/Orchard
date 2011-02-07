using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Orchard.Packaging.GalleryServer;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.Services {
    public interface IPackagingSourceManager : IDependency {
        IEnumerable<PackagingSource> GetSources();
        void AddSource(string feedTitle, string feedUrl);
        void RemoveSource(int id);

        IEnumerable<PackagingEntry> GetExtensionList(PackagingSource packagingSource = null, Func<IQueryable<PublishedPackage>, IQueryable<PublishedPackage>> query = null);
        int GetExtensionCount(PackagingSource packagingSource = null, Func<IQueryable<PublishedPackage>, IQueryable<PublishedPackage>> query = null);
    }
}