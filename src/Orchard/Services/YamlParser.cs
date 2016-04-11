using System.IO;
using YamlDotNet.Dynamic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Orchard.Services {
    public class YamlParser : IYamlParser {
        public dynamic Deserialize(string yaml) {
            return new DynamicYaml(yaml);
        }

        public T Deserialize<T>(string yaml) {
            var deserializer = new Deserializer(namingConvention: new PascalCaseNamingConvention(), ignoreUnmatched: true);
            using (var reader = new StringReader(yaml)) {
                return deserializer.Deserialize<T>(reader);
            }
        }
    }
}