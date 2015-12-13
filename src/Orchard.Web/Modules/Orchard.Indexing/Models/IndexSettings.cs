using System;
using System.Linq;
using System.Xml.Linq;

namespace Orchard.Indexing.Models
{
    public enum IndexingMode {
        Rebuild,
        Update
    }

    public class IndexSettings {
        public IndexingMode Mode { get; set; }
        public int LastIndexedId { get; set; }
        public int LastContentId { get; set; }
        public DateTime LastIndexedUtc { get; set; }

        public static readonly string TagSettings = "Settings";
        public static readonly string TagMode = "Mode";
        public static readonly string TagLastIndexedId = "LastIndexedId";
        public static readonly string TagLastContentId = "LastContentId";
        public static readonly string TagLastIndexedUtc = "LastIndexedUtc";

        public IndexSettings() {
            Mode = IndexingMode.Rebuild;
            LastIndexedId = 0;
            LastContentId = 0;
            LastIndexedUtc = DateTime.MinValue;
        }

        public static IndexSettings Parse(string content) {

            try {
                var doc = XDocument.Parse(content);

                return new IndexSettings {
                    Mode = (IndexingMode) Enum.Parse(typeof (IndexingMode), doc.Descendants(TagMode).First().Value),
                    LastIndexedId = Int32.Parse(doc.Descendants(TagLastIndexedId).First().Value),
                    LastContentId = Int32.Parse(doc.Descendants(TagLastContentId).First().Value),
                    LastIndexedUtc = DateTime.Parse(doc.Descendants(TagLastIndexedUtc).First().Value).ToUniversalTime()
                };
            }
            catch {
                return new IndexSettings();
            }
        }

        public string ToXml() {
            return new XDocument(
                    new XElement(TagSettings,
                        new XElement(TagMode, Mode),
                        new XElement(TagLastIndexedId, LastIndexedId),
                        new XElement(TagLastContentId, LastContentId),
                        new XElement(TagLastIndexedUtc, LastIndexedUtc.ToString("u"))
            )).ToString();
        }
    }
}