using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackagingSourceManager : IPackagingSourceManager {
        private static readonly XmlSerializer _sourceSerializer = new XmlSerializer(typeof(List<PackagingSource>), new XmlRootAttribute("Sources"));
        private readonly IAppDataFolder _appDataFolder;
        private readonly INotifier _notifier;

        public PackagingSourceManager(IAppDataFolder appDataFolder, INotifier notifier) {
            _appDataFolder = appDataFolder;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        #region IPackagingSourceManager Members

        public IEnumerable<PackagingSource> GetSources() {
            string text = _appDataFolder.ReadFile(GetSourcesPath());
            if ( string.IsNullOrEmpty(text) ) {
                return Enumerable.Empty<PackagingSource>();
            }

            var textReader = new StringReader(_appDataFolder.ReadFile(GetSourcesPath()));
            return (IEnumerable<PackagingSource>)_sourceSerializer.Deserialize(textReader);
        }

        public void AddSource(PackagingSource source) {
            SaveSources(GetSources().Concat(new[] { source }).GroupBy(x => x.FeedUrl).Select(g => g.First()));
        }

        public void RemoveSource(Guid id) {
            SaveSources(GetSources().Where(x => x.Id != id));
        }

        public void UpdateLists() {
            foreach ( PackagingSource source in GetSources() ) {
                UpdateSource(source);
            }
        }

        public IEnumerable<PackagingEntry> GetModuleList(PackagingSource packagingSource = null) {
            IEnumerable<PackagingEntry> packageInfos = ( packagingSource == null ? GetSources() : new[] { packagingSource } )
                .SelectMany(
                    source =>
                    Bind(ParseFeed(GetModuleListForSource(source)),
                         feed =>
                         feed.Items.SelectMany(
                             item =>
                             Unit(new PackagingEntry {
                                 Source = source,
                                 SyndicationFeed = feed,
                                 SyndicationItem = item,
                                 PackageStreamUri = item.Links.Single().GetAbsoluteUri().AbsoluteUri,
                             }))));


            return packageInfos.ToArray();
        }

        private string GetModuleListForSource(PackagingSource source) {
            if ( !_appDataFolder.FileExists(GetFeedCachePath(source)) ) {
                UpdateSource(source);
            }
            return _appDataFolder.ReadFile(GetFeedCachePath(source));
        }

        #endregion

        private static string GetSourcesPath() {
            return ".Packaging/Sources.xml";
        }

        private static string GetFeedCachePath(PackagingSource source) {
            return ".Packaging/Feed." + source.Id.ToString("n") + ".xml";
        }

        private void SaveSources(IEnumerable<PackagingSource> sources) {
            var textWriter = new StringWriter();
            _sourceSerializer.Serialize(textWriter, sources.ToList());

            _appDataFolder.CreateFile(GetSourcesPath(), textWriter.ToString());
        }

        private void UpdateSource(PackagingSource source) {
            try {
                XDocument feed = XDocument.Load(source.FeedUrl, LoadOptions.PreserveWhitespace);
                _appDataFolder.CreateFile(GetFeedCachePath(source), feed.ToString(SaveOptions.DisableFormatting));
            }
            catch ( Exception e ) {
                _notifier.Warning(T("Error loading content of feed '{0}': {1}", source.FeedUrl, e.Message));
            }
        }


        private static XName Atom(string localName) {
            return AtomExtensions.AtomXName(localName);
        }

        private static IEnumerable<T> Unit<T>(T t) where T : class {
            return t != null ? new[] { t } : Enumerable.Empty<T>();
        }

        private static IEnumerable<T2> Bind<T, T2>(T t, Func<T, IEnumerable<T2>> f) where T : class {
            return Unit(t).SelectMany(f);
        }

        private SyndicationFeed ParseFeed(string content) {
            if ( string.IsNullOrEmpty(( content )) )
                return new SyndicationFeed();

            var formatter = new Atom10FeedFormatter<SyndicationFeed>();
            formatter.ReadFrom(XmlReader.Create(new StringReader(content)));
            return formatter.Feed;
        }
    }
}