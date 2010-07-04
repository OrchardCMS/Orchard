using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.Serialization;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;

namespace Orchard.Modules.Packaging.Services {
    public interface IPackageRepository : IDependency {
        IEnumerable<PackageSource> GetSources();
        void AddSource(PackageSource source);
        void RemoveSource(Guid id);
        void UpdateLists();

        IEnumerable<PackageInfo> GetModuleList();
    }

    public class PackageSource {
        public Guid Id { get; set; }
        public string FeedUrl { get; set; }
    }

    public class PackageInfo {
        public PackageSource Source { get; set; }

        public AtomEntry AtomEntry { get; set; }
    }

    public class AtomEntry {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Updated { get; set; }
    }

    static class AtomExtensions {
        public static string Atom(this XElement entry, string localName) {
            var element = entry.Element(AtomXName(localName));
            return element != null ? element.Value : null;
        }

        public static XName AtomXName(string localName) {
            return XName.Get(localName, "http://www.w3.org/2005/Atom");
        }
    }

    [OrchardFeature("Orchard.Modules.Packaging")]
    public class PackageRepository : IPackageRepository {
        private readonly IAppDataFolder _appDataFolder;
        private static readonly XmlSerializer _sourceSerializer = new XmlSerializer(typeof(List<PackageSource>), new XmlRootAttribute("Sources"));

        public PackageRepository(IAppDataFolder appDataFolder) {
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

        public IEnumerable<PackageInfo> GetModuleList() {
            var packageInfos = GetSources()
                .SelectMany(
                    source =>
                    Bind(_appDataFolder.ReadFile(GetFeedCachePath(source)),
                         content =>
                         XDocument.Parse(content)
                             .Elements(Atom("feed"))
                             .Elements(Atom("entry"))
                             .SelectMany(
                                 element =>
                                 Bind(new AtomEntry {
                                     Id = element.Atom("id"),
                                     Title = element.Atom("title"),
                                     Updated = element.Atom("updated"),
                                 },
                                      atom =>
                                      Unit(new PackageInfo {
                                          Source = source,
                                          AtomEntry = atom,
                                      })))));

            return packageInfos.ToArray();
        }


        static IEnumerable<T> Unit<T>(T t) where T : class {
            return t != null ? new[] { t } : Enumerable.Empty<T>();
        }
        static IEnumerable<T2> Bind<T, T2>(T t, Func<T, IEnumerable<T2>> f) where T : class {
            return Unit(t).SelectMany(f);
        }
    }


}