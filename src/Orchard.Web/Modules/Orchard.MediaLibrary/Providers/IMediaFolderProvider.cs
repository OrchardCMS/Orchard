using Orchard.ContentManagement;

namespace Orchard.MediaLibrary.Providers {
    public interface IMediaFolderProvider : IDependency {
        string GetFolderName(ContentItem content);
    }
}