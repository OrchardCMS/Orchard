namespace Orchard.FileSystems.Media {
    public interface IMimeTypeProvider : IDependency {
        string GetMimeType(string path);
    }
}
