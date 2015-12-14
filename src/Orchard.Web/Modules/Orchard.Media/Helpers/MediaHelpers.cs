using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.Media.Models;

namespace Orchard.Media.Helpers {
    public static class MediaHelpers {
        public static IEnumerable<FolderNavigation> GetFolderNavigationHierarchy(string mediaPath) {
            List<FolderNavigation> navigations = new List<FolderNavigation>();
            if (String.IsNullOrEmpty(mediaPath)) {
                return navigations;
            }
            if ( !mediaPath.Contains(Path.DirectorySeparatorChar.ToString()) && !mediaPath.Contains(Path.AltDirectorySeparatorChar.ToString()) ) {
                navigations.Add(new FolderNavigation { FolderName = mediaPath, FolderPath = mediaPath });
                return navigations;
            }

            string[] navigationParts = mediaPath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries);
            string currentPath = String.Empty;
            foreach (string navigationPart in navigationParts) {
                currentPath = (string.IsNullOrEmpty(currentPath) ? navigationPart : currentPath + "\\" + navigationPart);
                navigations.Add(new FolderNavigation { FolderName = navigationPart, FolderPath = currentPath });
            }

            return navigations;
        }

        public static bool IsPicture(this HtmlHelper htmlHelper, string path) {
            return new[] {".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico"}
                .Contains((Path.GetExtension(path) ?? "").ToLowerInvariant());
        }
    }
}