using System.Collections.Generic;
using Orchard.Caching;

namespace Orchard.FileSystems.AppData {
    /// <summary>
    /// Abstraction of App_Data folder. All virtual paths passed in or returned are relative to "~/App_Data". 
    /// Expected to work on physical filesystem, but decouples core system from web hosting apis
    /// </summary>
    public interface IAppDataFolder : ISingletonDependency {
        /// <summary>
        /// Combines an array of strings into a path.
        /// </summary>
        /// <param name="paths">An relative of parts of the path.</param>
        /// <returns>The combined paths.</returns>
        string Combine(params string[] paths);

        /// <summary>
        /// Retrieves the list of files within an app folder directory.
        /// </summary>
        /// <param name="path">The relative path to the directory where to list the files.</param>
        /// <returns>The list of files within the directory.</returns>
        IEnumerable<string> ListFiles(string path);

        /// <summary>
        /// Retrieves the list of directories within an app folder directory.
        /// </summary>
        /// <param name="path">The relative path to the directory where to list the directories.</param>
        /// <returns>The list of directories within the directory.</returns>
        IEnumerable<string> ListDirectories(string path);

        /// <summary>
        /// Stores a file in the app data folder.
        /// </summary>
        /// <param name="path">The relative path to the file within the app data folder.</param>
        /// <param name="content">The content of the file to be stored.</param>
        void StoreFile(string path, string content);

        /// <summary>
        /// Opens a file for read from the app data folder.
        /// </summary>
        /// <param name="path">The relative path to the file within the app data folder.</param>
        /// <returns>A stream to the file.</returns>
        string ReadFile(string path);

        /// <summary>
        /// Deletes a file from the app data folder.
        /// </summary>
        /// <param name="path">The relative path to the file to be deleted.</param>
        void DeleteFile(string path);

        /// <summary>
        /// Returns a token to monitor a file for changes.
        /// </summary>
        /// <param name="path">The relative path to the folder within the app data folder.</param>
        /// <returns>The token to be monitored for changes in the file.</returns>
        IVolatileToken WhenPathChanges(string path);

        /// <summary>
        /// Maps a relative path to a physical path.
        /// </summary>
        /// <param name="path">The relative path within the app data folder.</param>
        /// <returns>The physical path to the file.</returns>
        string MapPath(string path);

        /// <summary>
        /// Retrieves a virtual path from a relative path within the app data folder.
        /// </summary>
        /// <param name="path">The relative path within the app data folder.</param>
        /// <returns>The correspondent virtual path.</returns>
        string GetVirtualPath(string path);
    }
}