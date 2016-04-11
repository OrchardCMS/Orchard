namespace Orchard.Services {
    /// <summary>
    /// Provides methods to deserialize objects from YAML documents.
    /// </summary>
    public interface IYamlParser : IDependency {
        /// <summary>
        /// Deserializes a YAML document to a dynamic object.
        /// </summary>
        /// <param name="yaml">The YAML document to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        dynamic Deserialize(string yaml);

        /// <summary>
        /// Deserializes a YAML document to a specific object.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="yaml">The YAML document to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        T Deserialize<T>(string yaml);
    }
}
