using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Packaging.GalleryServer;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.Services {
    /// <summary>
    /// Responsible for managing package sources and getting the list of packages from it.
    /// </summary>
    public interface IPackagingSourceManager : IDependency {
        /// <summary>
        /// Gets the different feed sources.
        /// </summary>
        /// <returns>The feeds.</returns>
        IEnumerable<PackagingSource> GetSources();

        /// <summary>
        /// Adds a new feed sources.
        /// </summary>
        /// <param name="feedTitle">The feed title.</param>
        /// <param name="feedUrl">The feed url.</param>
        /// <returns>The feed identifier.</returns>
        int AddSource(string feedTitle, string feedUrl);

        /// <summary>
        /// Removes a feed source.
        /// </summary>
        /// <param name="id">The feed identifier.</param>
        void RemoveSource(int id);

        /// <summary>
        /// Retrieves the list of extensions from a feed source.
        /// </summary>
        /// <param name="includeScreenshots">Specifies if screenshots should be included in the result.</param>
        /// <param name="packagingSource">The packaging source from where to get the extensions.</param>
        /// <param name="query">The optional query to retrieve the extensions.</param>
        /// <returns>The list of extensions.</returns>
        IEnumerable<PackagingEntry> GetExtensionList(bool includeScreenshots, PackagingSource packagingSource = null, Func<IQueryable<PublishedPackage>, IQueryable<PublishedPackage>> query = null);

        /// <summary>
        /// Retrieves the number of extensions from a feed source.
        /// </summary>
        /// <param name="packagingSource">The packaging source from where to get the extensions.</param>
        /// <param name="query">The optional query to retrieve the extensions.</param>
        /// <returns>The number of extensions from a feed source.</returns>
        int GetExtensionCount(PackagingSource packagingSource = null, Func<IQueryable<PublishedPackage>, IQueryable<PublishedPackage>> query = null);
    }
}