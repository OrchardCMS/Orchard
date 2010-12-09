using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Packaging.GalleryServer;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.Services {
    [OrchardFeature("Gallery")]
    public class PackagingSourceManager : IPackagingSourceManager {
        public const string ThemesPrefix = "Orchard.Themes.";
        public const string ModulesPrefix = "Orchard.Modules.";

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

        public IEnumerable<PackagingEntry> GetModuleList(PackagingSource packagingSource = null) {
            return GetExtensionList(DefaultExtensionTypes.Module, packagingSource);
        }
        public IEnumerable<PackagingEntry> GetThemeList(PackagingSource packagingSource = null) {
            return GetExtensionList(DefaultExtensionTypes.Theme, packagingSource);
        }

        private IEnumerable<PackagingEntry> GetExtensionList(string filter = null, PackagingSource packagingSource = null) {
            return (packagingSource == null ? GetSources() : new[] {packagingSource})
                .SelectMany(
                    source =>
                        new GalleryFeedContext(new Uri(source.FeedUrl)).Packages
                        .Where(p => p.PackageType == filter)
                        .ToList()
                        .Select(p => CreatePackageEntry(p, packagingSource))
                ).ToArray();
        }

        private static PackagingEntry CreatePackageEntry(PublishedPackage package, PackagingSource source) {
            return new PackagingEntry {
                Title = String.IsNullOrWhiteSpace(package.Title) ? package.Id : package.Title,
                PackageId = package.Id,
                PackageStreamUri = package.ProjectUrl != null ? package.ProjectUrl.ToString() : String.Empty,
                Source = source,
                Version = package.Version ?? String.Empty,
                Description = package.Description,
                Authors = package.Authors,
                LastUpdated = package.LastUpdated
            };
        }
        #endregion

    }
}