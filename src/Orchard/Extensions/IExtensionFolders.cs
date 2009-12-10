using System.Collections.Generic;
using Yaml.Grammar;

namespace Orchard.Extensions {
    public interface IExtensionFolders {
        IEnumerable<string> ListNames();
        ParseResult ParseManifest(string name);
    }

    public class ParseResult {
        public string Location { get; set; }
        public string Name { get; set; }
        public YamlDocument YamlDocument { get; set; }
    }
}