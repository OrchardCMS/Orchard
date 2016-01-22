using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Packaging.GalleryServer;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.Services {
    /// <summary>
    /// Responsible for managing package sources and getting the list of packages from it.
    /// </summary>
    [OrchardFeature("PackagingServices")]
    public class PackagingSourceManager : IPackagingSourceManager {
        public static string GetExtensionPrefix(string extensionType) {
            return string.Format("Orchard.{0}.", extensionType);
        }

        private readonly IRepository<PackagingSource> _packagingSourceRecordRepository;

        public PackagingSourceManager(IRepository<PackagingSource> packagingSourceRecordRepository) {
            _packagingSourceRecordRepository = packagingSourceRecordRepository;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        #region IPackagingSourceManager Members

        /// <summary>
        /// Gets the different feed sources.
        /// </summary>
        /// <returns>The feeds.</returns>
        public IEnumerable<PackagingSource> GetSources() {
            return _packagingSourceRecordRepository.Table.ToList();
        }

        /// <summary>
        /// Adds a new feed sources.
        /// </summary>
        /// <param name="feedTitle">The feed title.</param>
        /// <param name="feedUrl">The feed url.</param>
        /// <returns>The feed identifier.</returns>
        public int AddSource(string feedTitle, string feedUrl) {
            var packagingSource = new PackagingSource { FeedTitle = feedTitle, FeedUrl = feedUrl };

            _packagingSourceRecordRepository.Create(packagingSource);

            return packagingSource.Id;
        }

        /// <summary>
        /// Removes a feed source.
        /// </summary>
        /// <param name="id">The feed identifier.</param>
        public void RemoveSource(int id) {
            var packagingSource = _packagingSourceRecordRepository.Get(id);
            if(packagingSource != null) {
                _packagingSourceRecordRepository.Delete(packagingSource);
            }
        }

        /// <summary>
        /// Retrieves the list of extensions from a feed source.
        /// </summary>
        /// <param name="includeScreenshots">Specifies if screenshots should be included in the result.</param>
        /// <param name="packagingSource">The packaging source from where to get the extensions.</param>
        /// <param name="query">The optional query to retrieve the extensions.</param>
        /// <returns>The list of extensions.</returns>
        public IEnumerable<PackagingEntry> GetExtensionList(bool includeScreenshots, PackagingSource packagingSource = null, Func<IQueryable<PublishedPackage>, IQueryable<PublishedPackage>> query = null) {
            return (packagingSource == null ? GetSources() : new[] {packagingSource})
                .SelectMany(source => GetExtensionListFromSource(includeScreenshots, packagingSource, query, source));
        }

        private static IEnumerable<PackagingEntry> GetExtensionListFromSource(bool includeScreenshots, PackagingSource packagingSource, Func<IQueryable<PublishedPackage>, IQueryable<PublishedPackage>> query, PackagingSource source) {
            var galleryFeedContext = new GalleryFeedContext(new Uri(source.FeedUrl)) { IgnoreMissingProperties = true };

            // Setup compression
            galleryFeedContext.SendingRequest += (o, e) => {
                if (e.Request is HttpWebRequest) {
                    (e.Request as HttpWebRequest).AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
            };

            // Include screenshots if needed
            IQueryable<PublishedPackage> packages = includeScreenshots
                ? galleryFeedContext.Packages.Expand("Screenshots")
                : galleryFeedContext.Packages;
                        
            if (query != null) {
                packages = query(packages);
            }

            return packages.ToList().Select(p => CreatePackageEntry(p, packagingSource, galleryFeedContext.GetReadStreamUri(p)));
        }

        /// <summary>
        /// Retrieves the number of extensions from a feed source.
        /// </summary>
        /// <param name="packagingSource">The packaging source from where to get the extensions.</param>
        /// <param name="query">The optional query to retrieve the extensions.</param>
        /// <returns>The number of extensions from a feed source.</returns>
        public int GetExtensionCount(PackagingSource packagingSource = null, Func<IQueryable<PublishedPackage>, IQueryable<PublishedPackage>> query = null) {
            return (packagingSource == null ? GetSources() : new[] { packagingSource })
                .Sum( source => {
                        var galleryFeedContext = new GalleryFeedContext(new Uri(source.FeedUrl)) { IgnoreMissingProperties = true };
                        IQueryable<PublishedPackage> packages = galleryFeedContext.Packages;

                        if (query != null) {
                            packages = query(packages);
                        }

                        return packages.Count();
                    }
                );
        }

        #endregion

        private static PackagingEntry CreatePackageEntry(PublishedPackage package, PackagingSource source, Uri downloadUri) {
            var baseUri = new Uri(string.Format("{0}://{1}:{2}/",
                                                downloadUri.Scheme,
                                                downloadUri.Host,
                                                downloadUri.Port));

            PublishedScreenshot screenshot = package.Screenshots != null ? package.Screenshots.FirstOrDefault() : null;

            string iconUrl = GetAbsoluteUri(package.IconUrl, baseUri);
            string firstScreenshot = screenshot != null ? GetAbsoluteUri(screenshot.ScreenshotUri, baseUri) : string.Empty;

            return new PackagingEntry {
                Title = string.IsNullOrWhiteSpace(package.Title) ? package.Id : package.Title,
                PackageId = package.Id,
                PackageStreamUri = downloadUri.ToString(),
                ProjectUrl = package.ProjectUrl,
                GalleryDetailsUrl = package.GalleryDetailsUrl,
                Source = source,
                Version = package.Version ?? string.Empty,
                Description = package.Description,
                Authors = package.Authors,
                LastUpdated = package.LastUpdated,
                IconUrl = iconUrl,
                FirstScreenshot = firstScreenshot,
                Rating = package.Rating,
                RatingsCount = package.RatingsCount,
                DownloadCount = package.DownloadCount
            };
        }

        protected static string GetAbsoluteUri(string url, Uri baseUri) {
            Uri uri = null;
            if (!string.IsNullOrEmpty(url)) {
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri)) {
                    Uri.TryCreate(baseUri,
                        url,
                        out uri);
                }
            }

            return uri != null ? uri.ToString() : string.Empty;
        }
    }
}