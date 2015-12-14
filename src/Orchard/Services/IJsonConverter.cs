namespace Orchard.Services {
    /// <summary>
    /// Provides services to serialize and deserialize objects to and from
    /// Json documents.
    /// </summary>
    public interface IJsonConverter : IDependency {
        /// <summary>
        /// Serializes an object to Json.
        /// </summary>
        /// <param name="o">The object to serialize.</param>
        /// <returns>A string representing the object as Json.</returns>
        string Serialize(object o);

        /// <summary>
        /// Serializes an object to Json using an optional indentation.
        /// </summary>
        /// <param name="o">The object to serialize.</param>
        /// <param name="format">Whether the document should be indented.</param>
        /// <returns>A string representing the object as Json.</returns>
        string Serialize(object o, JsonFormat format);

        /// <summary>
        /// Deserializes a Json document to a dynamic object.
        /// </summary>
        /// <param name="json">The Json document to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        dynamic Deserialize(string json);

        /// <summary>
        /// Deserializes a Json document to a specific object.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="json">The Json document to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        T Deserialize<T>(string json);
    }

    public enum JsonFormat {
        None,
        Indented
    }
}
