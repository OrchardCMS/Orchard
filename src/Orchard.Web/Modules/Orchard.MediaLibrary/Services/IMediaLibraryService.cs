using System.Collections.Generic;
using System.IO;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Services {
    public interface IMediaLibraryService : IDependency {
        /// <summary>
        /// Returns the whole hierarchy of media folders
        /// </summary>
        /// <returns></returns>
        IEnumerable<MediaFolder> GetMediaFolders();

        MediaFolder GetMediaFolder(int id);
        IEnumerable<MediaFolder> GetMediaFolderHierarchy(int id);

        /// <summary>
        /// Returns the list of all Media Content Types
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetMediaTypes();

        IEnumerable<MediaPart> GetMediaContentItemsForLocation(int? locationId, int skip, int count);
        int GetMediaContentItemsCountForLocation(int? locationId);

        MediaPart ImportStream(int termId, Stream stream, string filename);

        void CreateFolder(int? parentFolderId, string name);
        void RenameFolder(int folderId, string name);
        void DeleteFolder(int folderId);
        void MoveMedia(int targetId, int[] mediaItemIds);
    }
}