using Orchard.Security;

namespace Orchard.MediaLibrary.Providers {
    public interface IMediaFolderProvider : IDependency {
        string GetFolderName(IUser content);
    }
}