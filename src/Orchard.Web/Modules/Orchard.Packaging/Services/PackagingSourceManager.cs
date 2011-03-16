using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Packaging.GalleryServer;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.Services {
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

        Localizer T { get; set; }

        #region IPackagingSourceManager Members

        public IEnumerable<PackagingSource> GetSources() {
            return _packagingSourceRecordRepository.Table.ToList();
        }

        public void AddSource(string feedTitle, string feedUrl) {
            var packagingSource = new PackagingSource {FeedTitle = feedTitle, FeedUrl = feedUrl};
            _packagingSourceRecordRepository.Create(packagingSource);
        }

        public void RemoveSource(int id) {
            var packagingSource = _packagingSourceRecordRepository.Get(id);
            if(packagingSource != null) {
                _packagingSourceRecordRepository.Delete(packagingSource);
            }
        }

        public IEnumerable<PackagingEntry> GetExtensionList(PackagingSource packagingSource = null, Func<IQueryable<PublishedPackage>, IQueryable<PublishedPackage>> query = null) {
            return (packagingSource == null ? GetSources() : new[] {packagingSource})
                .SelectMany(
                    source => {
                        var galleryFeedContext = new GalleryFeedContext(new Uri(source.FeedUrl));
                        IQueryable<PublishedPackage> packages = galleryFeedContext.Packages;

                        if (query != null) {
                            packages = query(packages);
                        }

                        return packages.ToList().Select(
                            p => {
                                PublishedScreenshot firstScreenshot = galleryFeedContext.Screenshots
                                    .Where(s => s.PublishedPackageId == p.Id && s.PublishedPackageVersion == p.Version)
                                    .ToList()
                                    .FirstOrDefault();
                                return CreatePackageEntry(p, firstScreenshot, packagingSource, galleryFeedContext.GetReadStreamUri(p));
                            });
                    }
                );
        }

        public int GetExtensionCount(PackagingSource packagingSource = null, Func<IQueryable<PublishedPackage>, IQueryable<PublishedPackage>> query = null) {
            return (packagingSource == null ? GetSources() : new[] { packagingSource })
                .Sum( source => {
                        var galleryFeedContext = new GalleryFeedContext(new Uri(source.FeedUrl));
                        IQueryable<PublishedPackage> packages = galleryFeedContext.Packages;

                        if (query != null) {
                            packages = query(packages);
                        }

                        return packages.Count();
                    }
                );
        }

        private static PackagingEntry CreatePackageEntry(PublishedPackage package, PublishedScreenshot screenshot, PackagingSource source, Uri downloadUri) {
            Uri baseUri = new Uri(string.Format("{0}://{1}:{2}/",
                                                downloadUri.Scheme,
                                                downloadUri.Host,
                                                downloadUri.Port));

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
            if (!string.IsNullOrEmpty(url))
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    Uri.TryCreate(baseUri,
                        url,
                        out uri);
                }
            }

            return uri != null ? uri.ToString() : string.Empty;
        }

        #endregion
    }
}