using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Orchard.FileSystems.AppData;

namespace Orchard.Packaging {
    public class PackageSourceManager : IPackageSourceManager {
        private readonly IAppDataFolder _appDataFolder;
        private static readonly XmlSerializer _sourceSerializer = new XmlSerializer(typeof(List<PackageSource>), new XmlRootAttribute("Sources"));

        public PackageSourceManager(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
        }

        static string GetSourcesPath() {
            return ".Packaging/Sources.xml";
        }
        static string GetFeedCachePath(PackageSource source) {
            return ".Packaging/Feed." + source.Id.ToString("n") + ".xml";
        }

        public IEnumerable<PackageSource> GetSources() {
            var text = _appDataFolder.ReadFile(GetSourcesPath());
            if (string.IsNullOrEmpty(text))
                return Enumerable.Empty<PackageSource>();

            var textReader = new StringReader(_appDataFolder.ReadFile(GetSourcesPath()));
            return (IEnumerable<PackageSource>)_sourceSerializer.Deserialize(textReader);
        }

        void SaveSources(IEnumerable<PackageSource> sources) {
            var textWriter = new StringWriter();
            _sourceSerializer.Serialize(textWriter, sources.ToList());

            _appDataFolder.CreateFile(GetSourcesPath(), textWriter.ToString());
        }

        public void AddSource(PackageSource source) {
            UpdateSource(source);
            SaveSources(GetSources().Concat(new[] { source }));
        }

        public void RemoveSource(Guid id) {
            SaveSources(GetSources().Where(x => x.Id != id));
        }

        public void UpdateLists() {
            foreach (var source in GetSources()) {
                UpdateSource(source);
            }
        }

        private void UpdateSource(PackageSource source) {
            var feed = XDocument.Load(source.FeedUrl, LoadOptions.PreserveWhitespace);
            _appDataFolder.CreateFile(GetFeedCachePath(source), feed.ToString(SaveOptions.DisableFormatting));
        }


        static XName Atom(string localName) {
            return AtomExtensions.AtomXName(localName);
        }

        static IEnumerable<T> Unit<T>(T t) where T : class {
            return t != null ? new[] { t } : Enumerable.Empty<T>();
        }
        static IEnumerable<T2> Bind<T, T2>(T t, Func<T, IEnumerable<T2>> f) where T : class {
            return Unit(t).SelectMany(f);
        }

        private SyndicationFeed ParseFeed(string content) {
            var formatter = new Atom10FeedFormatter<SyndicationFeed>();
            formatter.ReadFrom(XmlReader.Create(new StringReader(content)));
            return formatter.Feed;
        }

        public IEnumerable<PackageEntry> GetModuleList() {
            var packageInfos = GetSources()
                .SelectMany(
                    source =>
                    Bind(ParseFeed(_appDataFolder.ReadFile(GetFeedCachePath(source))),
                         feed =>
                             feed.Items.SelectMany(
                             item =>
                                 Unit(new PackageEntry {
                                     Source = source,
                                     SyndicationFeed = feed,
                                     SyndicationItem = item,
                                     PackageStreamUri = item.Links.Single().GetAbsoluteUri().AbsoluteUri,
                                 }))));


            return packageInfos.ToArray();
        }
    }
}