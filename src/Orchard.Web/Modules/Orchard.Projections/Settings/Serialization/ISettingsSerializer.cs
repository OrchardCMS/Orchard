using System.IO;

namespace Orchard.Projections.Settings.Serialization {
    public interface ISettingsSerializer : IDependency {
        void Serialize(TextWriter tw, SObject o);
        SObject Deserialize(TextReader tr);
    }
}