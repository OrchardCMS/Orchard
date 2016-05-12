using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using NHibernate.Hql;
using Orchard.Environment;

namespace Orchard.Validation {
    /// <summary>
    /// Provides methods to validate paths.
    /// </summary>
    public static class PathValidation {

        /// <summary>
        /// Determines if a path lies within the base path boundaries.
        /// If not, an exception is thrown.
        /// </summary>
        /// <param name="basePath">The base path which boundaries are not to be transposed.</param>
        /// <param name="mappedPath">The path to determine.</param>
        /// <rereturns>The mapped path if valid.</rereturns>
        /// <exception cref="ArgumentException">If the path is invalid.</exception>
        public static string ValidatePath(string basePath, string mappedPath) {
            bool valid = false;

            try {
                // Check that we are indeed within the storage directory boundaries
                valid = Path.GetFullPath(mappedPath).StartsWith(Path.GetFullPath(basePath), StringComparison.OrdinalIgnoreCase);
            } catch {
                // Make sure that if invalid for medium trust we give a proper exception
                valid = false;
            }

            if (!valid) {
                if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
                    throw new InvalidWindowsPathException("Invalid path");
                else
                    throw new ArgumentException("Invalid path");
            }

            return mappedPath;
        }
    }

    /// <summary>
    /// Thrown when an invalid path is encountered on a Windows platform.
    /// </summary>
    [Serializable]
    public class InvalidWindowsPathException : ApplicationException {
        public InvalidWindowsPathException(string message) : base(message) {
        }
    }
}
