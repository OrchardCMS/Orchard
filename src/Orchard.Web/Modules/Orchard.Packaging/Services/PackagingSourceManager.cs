using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackagingSourceManager : IPackagingSourceManager {
        private const string ModulesFilter = "Orchard.Module.";
        private const string ThemesFilter = "Orchard.Theme.";

        private readonly IRepository<PackagingSourceRecord> _packagingSourceRecordRepository;

        public PackagingSourceManager(IRepository<PackagingSourceRecord> packagingSourceRecordRepository) {
            _packagingSourceRecordRepository = packagingSourceRecordRepository;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        #region IPackagingSourceManager Members

        public IEnumerable<PackagingSourceRecord> GetSources() {
            return _packagingSourceRecordRepository.Table.ToList();
        }

        public void AddSource(string feedTitle, string feedUrl) {
            _packagingSourceRecordRepository.Create(new PackagingSourceRecord {FeedTitle = feedTitle, FeedUrl = feedUrl});
        }

        public void RemoveSource(int id) {
            var packagingSource = _packagingSourceRecordRepository.Get(id);
            if(packagingSource != null) {
                _packagingSourceRecordRepository.Delete(packagingSource);
            }
        }

        public IEnumerable<PackagingEntry> GetModuleList(PackagingSourceRecord packagingSource = null) {
            return GetExtensionList(ModulesFilter, packagingSource);
        }
        public IEnumerable<PackagingEntry> GetThemeList(PackagingSourceRecord packagingSource = null) {
            return GetExtensionList(ThemesFilter, packagingSource);
        }

        private IEnumerable<PackagingEntry> GetExtensionList(string filter = null, PackagingSourceRecord packagingSource = null) {
            return ( packagingSource == null ? GetSources() : new[] { packagingSource } )
                .SelectMany(
                    source =>
                    new DataServicePackageRepository(new Uri(source.FeedUrl))
                        .GetPackages()
                        .Where(p => p.Id.StartsWith(filter ?? String.Empty))
                        .ToList()
                        .Select(p => new PackagingEntry {
                            Title = String.IsNullOrWhiteSpace(p.Title) ? p.Id : p.Title,
                            PackageId = p.Id,
                            PackageStreamUri = p.ProjectUrl != null ? p.ProjectUrl.ToString() : String.Empty,
                            Source = source,
                            Version = p.Version != null ? p.Version.ToString() : String.Empty,
                            Description = p.Description,
                            Authors = p.Authors != null ? String.Join(", ", p.Authors) : String.Empty,
                        })
                ).ToArray();
        }

        #endregion

    }
}