using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Logging;
using Orchard.Utility;
using Yaml.Grammar;

namespace Orchard.Pages.Services.Templates {
    public interface ITemplateMetadataParser : IDependency {
        IList<MetadataEntry> Parse(TextReader reader);
    }

    /// <summary>
    /// Parse the content of a text reader into a list of metadata entries.
    /// </summary>
    public class TemplateMetadataParser : ITemplateMetadataParser {
        public TemplateMetadataParser() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        private static IList<MetadataEntry> EmptyList { get { return new MetadataEntry[0]; } }

        public IList<MetadataEntry> Parse(TextReader reader) {
            string content = reader.ReadToEnd();

            var yamlInput = new TextInput(content);

            var parser = new YamlParser();

            bool success;
            var stream = parser.ParseYamlStream(yamlInput, out success);
            if (!success) {
                Logger.Error("Error parsing template metadata. Detailed YAML parsing error: {0}", parser.ErrorMessages);
                return EmptyList;
            }

            if (stream.Documents.Count == 0) {
                Logger.Warning("No template metadata found.");
                return EmptyList;
            }

            if (stream.Documents.Count >= 2) {
                Logger.Information("Some entries where ignored in the template metadata because the metadata text contains more than one YAML 'document'.");
            }

            YamlDocument doc = stream.Documents.Single();

            var root = doc.Root as Mapping;
            if (root == null) {
                Logger.Error("Invalid template metadata: The YAML root element is not a 'mapping'.");
                return EmptyList;
            }

            var result = root.Entities
                .Where(x => x.Key is Scalar && x.Value is Scalar)
                .Select(x => new MetadataEntry { Tag = ((Scalar)x.Key).Text, Value = ((Scalar)x.Value).Text })
                .ToReadOnlyCollection();

            if (root.Entities.Count != result.Count) {
                Logger.Information("Some entries were ignored in template metadata because they are not YAML 'scalars'.");
            }

            return result;
        }
    }
}
